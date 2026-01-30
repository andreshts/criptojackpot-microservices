# =============================================================================
# Main Configuration - CryptoJackpot DigitalOcean Infrastructure
# =============================================================================

locals {
  common_tags = concat(var.tags, ["env:${var.environment}"])
  
  # Nombres de recursos con prefijo de ambiente
  resource_prefix = "${var.project_name}-${var.environment}"
}

# -----------------------------------------------------------------------------
# Generación de secretos aleatorios (si no se proporcionan)
# -----------------------------------------------------------------------------
resource "random_password" "jwt_secret" {
  count   = var.jwt_secret_key == "" ? 1 : 0
  length  = 64
  special = true
}

resource "random_password" "kafka_password" {
  count   = var.kafka_password == "" ? 1 : 0
  length  = 32
  special = false
}

resource "random_password" "redpanda_admin_password" {
  length  = 32
  special = false
}

locals {
  jwt_secret_key         = var.jwt_secret_key != "" ? var.jwt_secret_key : random_password.jwt_secret[0].result
  kafka_app_password     = var.kafka_password != "" ? var.kafka_password : random_password.kafka_password[0].result
  redpanda_admin_password = random_password.redpanda_admin_password.result
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

  # Node Pool Configuration
  node_pool_name  = "${local.resource_prefix}-workers"
  node_size       = var.k8s_node_size
  node_count      = var.k8s_node_count
  auto_scale      = var.k8s_auto_scale
  min_nodes       = var.k8s_min_nodes
  max_nodes       = var.k8s_max_nodes

  tags = local.common_tags
}

# -----------------------------------------------------------------------------
# DOCR (Container Registry) Module
# -----------------------------------------------------------------------------
module "docr" {
  source = "./modules/docr"

  name                  = var.project_name
  subscription_tier     = var.registry_subscription_tier
  region                = var.region
  kubernetes_cluster_id = module.doks.cluster_id
}

# -----------------------------------------------------------------------------
# Database (PostgreSQL) Module
# -----------------------------------------------------------------------------
module "database" {
  source = "./modules/database"

  name       = "${local.resource_prefix}-db"
  region     = var.region
  size       = var.db_size
  node_count = var.db_node_count
  version_pg = var.db_version
  vpc_uuid   = module.vpc.vpc_id

  # Lista de bases de datos a crear
  databases = var.databases

  # Trusted sources - solo el cluster puede acceder
  trusted_sources_ids = [module.doks.cluster_id]

  tags = local.common_tags
}

# -----------------------------------------------------------------------------
# Redis (SignalR Backplane) Module
# -----------------------------------------------------------------------------
module "redis" {
  source = "./modules/redis"

  name          = "${local.resource_prefix}-redis"
  region        = var.region
  size          = var.redis_size
  node_count    = var.redis_node_count
  version_redis = var.redis_version
  vpc_uuid      = module.vpc.vpc_id

  # Eviction policy para SignalR (LRU es ideal)
  eviction_policy = "allkeys_lru"

  # Trusted sources - solo el cluster puede acceder
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

  # CORS para frontend
  cors_allowed_origins = ["https://${var.domain}", "https://www.${var.domain}"]

  # ⚠️ CUIDADO: Solo habilitar en desarrollo
  # En producción SIEMPRE debe ser false para proteger datos de usuarios
  force_destroy = var.spaces_force_destroy
}

# -----------------------------------------------------------------------------
# Kubernetes Secrets Module
# -----------------------------------------------------------------------------
module "k8s_secrets" {
  source = "./modules/secrets"

  depends_on = [module.doks, module.database, module.spaces, module.redis]

  namespace = var.project_name

  # PostgreSQL Configuration
  postgres_host     = module.database.host
  postgres_port     = module.database.port
  postgres_user     = module.database.user
  postgres_password = module.database.password
  databases         = var.databases

  # JWT Configuration
  jwt_secret_key = local.jwt_secret_key
  jwt_issuer     = var.jwt_issuer
  jwt_audience   = var.jwt_audience

  # Kafka/Redpanda Configuration
  kafka_bootstrap_servers = "redpanda.${var.project_name}.svc.cluster.local:9092"
  kafka_app_username      = "${var.project_name}-app"
  kafka_app_password      = local.kafka_app_password
  redpanda_admin_password = local.redpanda_admin_password

  # Redis Configuration (SignalR Backplane)
  redis_connection_string = module.redis.connection_string

  # DigitalOcean Spaces Configuration
  spaces_endpoint   = module.spaces.endpoint
  spaces_region     = var.region
  spaces_bucket     = module.spaces.bucket_name
  spaces_access_key = var.spaces_access_key
  spaces_secret_key = var.spaces_secret_key
}

# -----------------------------------------------------------------------------
# Helm Releases (Nginx Ingress + Cert-Manager)
# -----------------------------------------------------------------------------
module "ingress" {
  source = "./modules/ingress"

  depends_on = [module.doks]

  enable_ssl        = var.enable_ssl
  letsencrypt_email = var.letsencrypt_email
}

# -----------------------------------------------------------------------------
# Output de secrets.yaml generado
# -----------------------------------------------------------------------------
resource "local_file" "secrets_yaml" {
  content  = module.k8s_secrets.secrets_yaml_content
  filename = "${path.root}/../k8s/base/secrets.generated.yaml"

  file_permission = "0600"
}

resource "local_file" "deploy_config" {
  content = templatefile("${path.module}/templates/deploy-config.tpl", {
    registry_url   = module.docr.registry_url
    registry_name  = module.docr.registry_name
    cluster_name   = module.doks.cluster_name
    cluster_id     = module.doks.cluster_id
    region         = var.region
    environment    = var.environment
  })
  filename = "${path.root}/../deploy-config.json"
}

# -----------------------------------------------------------------------------
# Cloudflare DNS Automation
# -----------------------------------------------------------------------------

# Extraemos el subdominio de tu variable "domain" (api.cryptojackpot.com -> api)
# Asumiendo que tu zona es cryptojackpot.com
locals {
  # Si var.domain es "api.cryptojackpot.com", esto extrae "api"
  # Si var.domain es "dev-api.cryptojackpot.com", esto extrae "dev-api"
  domain_parts       = split(".", var.domain)
  domain_name_part   = local.domain_parts[0]
  is_cloudflare_ready = var.enable_cloudflare_dns && var.cloudflare_api_token != "" && var.cloudflare_zone_id != ""
}

# Registro DNS tipo A apuntando al Load Balancer de NGINX Ingress
resource "cloudflare_record" "api_endpoint" {
  count = local.is_cloudflare_ready && module.ingress.load_balancer_ip != "pending" ? 1 : 0

  zone_id = var.cloudflare_zone_id
  name    = local.domain_name_part # Ej: "api" o "dev-api"
  content = module.ingress.load_balancer_ip
  type    = "A"
  proxied = var.cloudflare_proxied # True activa el WAF y CDN (Nube Naranja)
  ttl     = var.cloudflare_proxied ? 1 : 300 # TTL automático si está proxied
  
  comment = "Managed by Terraform - ${var.project_name} ${var.environment}"
}

