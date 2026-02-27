# =============================================================================
# Main Configuration - CriptoJackpot DigitalOcean Infrastructure
# =============================================================================

locals {
  common_tags = concat(var.tags, ["env:${var.environment}"])

  # Nombres de recursos con prefijo de ambiente
  resource_prefix = "${var.project_name}-${var.environment}"

  # Subdominio para el DNS record (api-qa o api)
  domain_parts     = split(".", var.domain)
  subdomain        = local.domain_parts[0]                          # "api-qa" / "api"
  zone_domain      = join(".", slice(local.domain_parts, 1, length(local.domain_parts))) # "criptojackpot.com"

  is_cloudflare_ready = var.enable_cloudflare_dns && var.cloudflare_api_token != "" && var.cloudflare_zone_id != ""

  # Frontend URL para emails (qa vs prod)
  frontend_url = var.environment == "prod" ? "https://criptojackpot.com" : "https://qa.criptojackpot.com"
}

# -----------------------------------------------------------------------------
# Generación de secretos aleatorios (si no se proporcionan)
# -----------------------------------------------------------------------------
resource "random_password" "jwt_secret" {
  count   = var.jwt_secret_key == "" ? 1 : 0
  length  = 64
  special = false # Sin especiales para evitar problemas con connection strings
}

locals {
  jwt_secret_key = var.jwt_secret_key != "" ? var.jwt_secret_key : random_password.jwt_secret[0].result
}

# -----------------------------------------------------------------------------
# VPC Module
# -----------------------------------------------------------------------------
module "vpc" {
  source = "./modules/vpc"

  name        = "${local.resource_prefix}-vpc"
  region      = var.region
  ip_range    = var.vpc_ip_range
  description = "VPC para ${var.project_name} - ${var.environment}"
}

# -----------------------------------------------------------------------------
# DOKS (Kubernetes) Module
# -----------------------------------------------------------------------------
module "doks" {
  source = "./modules/doks"

  name        = "${local.resource_prefix}-cluster"
  region      = var.region
  version_k8s = var.k8s_version
  vpc_uuid    = module.vpc.vpc_id

  node_pool_name = "${local.resource_prefix}-workers"
  node_size      = var.k8s_node_size
  node_count     = var.k8s_node_count
  auto_scale     = var.k8s_auto_scale
  min_nodes      = var.k8s_min_nodes
  max_nodes      = var.k8s_max_nodes

  tags = local.common_tags
}

# -----------------------------------------------------------------------------
# DOCR (Container Registry) Module
# Compartido entre QA y Prod — se crea una sola vez con environment="prod"
# En QA usa el mismo registry pero con tags diferentes (:qa vs :v1.x.x)
# -----------------------------------------------------------------------------
module "docr" {
  source = "./modules/docr"

  name                  = var.project_name
  subscription_tier     = var.registry_subscription_tier
  region                = var.region
  kubernetes_cluster_id = module.doks.cluster_id
}

# -----------------------------------------------------------------------------
# Database (DO Managed PostgreSQL) Module
# -----------------------------------------------------------------------------
module "database" {
  source = "./modules/database"

  name       = "${local.resource_prefix}-db"
  region     = var.region
  size       = var.db_size
  node_count = var.db_node_count
  version_pg = var.db_version
  vpc_uuid   = module.vpc.vpc_id

  databases = var.databases

  # Solo el cluster DOKS puede acceder a la DB
  trusted_sources_ids = [module.doks.cluster_id]

  tags = local.common_tags
}

# -----------------------------------------------------------------------------
# Spaces (Object Storage) Module
# -----------------------------------------------------------------------------
module "spaces" {
  source = "./modules/spaces"

  name   = var.spaces_bucket_name
  region = var.region
  acl    = var.spaces_acl

  cors_allowed_origins = [local.frontend_url]
  force_destroy        = var.spaces_force_destroy
}

# -----------------------------------------------------------------------------
# NGINX Ingress Controller (sin cert-manager — TLS gestionado por Cloudflare)
# -----------------------------------------------------------------------------
module "ingress" {
  source = "./modules/ingress"

  depends_on = [module.doks]

  # Cloudflare termina TLS externamente — cert-manager no se usa
  enable_ssl = false

  ingress_replicas = var.environment == "prod" ? 2 : 1
}

# -----------------------------------------------------------------------------
# Kubernetes Secrets Module
# Crea los secrets reales en el cluster con valores de los servicios gestionados
# -----------------------------------------------------------------------------
module "k8s_secrets" {
  source = "./modules/secrets"

  depends_on = [module.doks, module.database, module.spaces]

  namespace   = var.project_name
  environment = var.environment

  # PostgreSQL - DO Managed (microservicios conectan via PgBouncer interno)
  postgres_host     = module.database.host
  postgres_port     = module.database.port
  postgres_user     = module.database.user
  postgres_password = module.database.password
  databases         = var.databases

  # JWT
  jwt_secret_key = local.jwt_secret_key
  jwt_issuer     = var.jwt_issuer
  jwt_audience   = var.jwt_audience

  # Kafka - Upstash (SASL_SSL externo)
  kafka_bootstrap_servers = var.kafka_bootstrap_servers
  kafka_sasl_username     = var.kafka_sasl_username
  kafka_sasl_password     = var.kafka_sasl_password
  kafka_sasl_mechanism    = var.kafka_sasl_mechanism
  kafka_security_protocol = var.kafka_security_protocol

  # Redis - Upstash (externo, TLS)
  redis_connection_string = var.redis_connection_string

  # MongoDB Atlas - Audit service
  mongodb_connection_string = var.mongodb_connection_string
  mongodb_audit_database    = var.mongodb_audit_database

  # DigitalOcean Spaces
  spaces_endpoint   = module.spaces.endpoint
  spaces_region     = var.region
  spaces_bucket     = module.spaces.bucket_name
  spaces_access_key = var.spaces_access_key
  spaces_secret_key = var.spaces_secret_key

  # Brevo - Notification service
  brevo_api_key      = var.brevo_api_key
  brevo_sender_email = var.brevo_sender_email
  brevo_sender_name  = var.brevo_sender_name
  brevo_base_url     = local.frontend_url
}

# -----------------------------------------------------------------------------
# Kustomize apply — despliega los manifiestos K8s del overlay correcto
# Se ejecuta DESPUÉS de que el cluster y los secrets estén listos
# -----------------------------------------------------------------------------
resource "null_resource" "kustomize_apply" {
  depends_on = [module.doks, module.k8s_secrets, module.ingress]

  triggers = {
    # Re-aplica si el cluster cambia o si hay cambios en los overlays
    cluster_id  = module.doks.cluster_id
    environment = var.environment
    # Checksum de los archivos del overlay para detectar cambios
    overlay_hash = sha256(join("", [
      filesha256("${path.root}/../k8s/overlays/${var.environment}/kustomization.yaml"),
    ]))
  }

  provisioner "local-exec" {
    command     = <<-EOT
      echo "Conectando kubectl al cluster ${module.doks.cluster_name}..."
      doctl kubernetes cluster kubeconfig save ${module.doks.cluster_id} --context ${local.resource_prefix}

      echo "Aplicando manifiestos Kustomize para ambiente: ${var.environment}..."
      kubectl apply -k ../k8s/overlays/${var.environment} --context ${local.resource_prefix} --timeout=300s

      echo "Verificando rollout de deployments..."
      kubectl rollout status deployment/bff-gateway -n ${var.project_name} --context ${local.resource_prefix} --timeout=300s
    EOT
    interpreter = ["bash", "-c"]
    working_dir = path.module
  }
}

# -----------------------------------------------------------------------------
# Cloudflare DNS — apunta al Load Balancer IP del NGINX Ingress
# -----------------------------------------------------------------------------
resource "cloudflare_record" "api_endpoint" {
  count = local.is_cloudflare_ready && module.ingress.load_balancer_ip != "pending" ? 1 : 0

  zone_id = var.cloudflare_zone_id
  name    = local.subdomain  # "api-qa" o "api"
  content = module.ingress.load_balancer_ip
  type    = "A"
  proxied = var.cloudflare_proxied
  ttl     = 1  # Automático cuando proxied=true

  comment = "Managed by Terraform - ${var.project_name} ${var.environment}"
}

# -----------------------------------------------------------------------------
# Archivos de salida útiles para CI/CD
# -----------------------------------------------------------------------------
resource "local_file" "deploy_config" {
  content = templatefile("${path.module}/templates/deploy-config.tpl", {
    registry_url   = module.docr.registry_url
    registry_name  = module.docr.registry_name
    cluster_name   = module.doks.cluster_name
    cluster_id     = module.doks.cluster_id
    region         = var.region
    environment    = var.environment
  })
  filename = "${path.root}/../deploy-config.${var.environment}.json"
}
