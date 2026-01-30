# =============================================================================
# Outputs - CryptoJackpot DigitalOcean Infrastructure
# =============================================================================

# -----------------------------------------------------------------------------
# VPC Outputs
# -----------------------------------------------------------------------------
output "vpc_id" {
  description = "ID de la VPC"
  value       = module.vpc.vpc_id
}

output "vpc_urn" {
  description = "URN de la VPC"
  value       = module.vpc.vpc_urn
}

# -----------------------------------------------------------------------------
# Kubernetes Outputs
# -----------------------------------------------------------------------------
output "cluster_id" {
  description = "ID del cluster DOKS"
  value       = module.doks.cluster_id
}

output "cluster_name" {
  description = "Nombre del cluster DOKS"
  value       = module.doks.cluster_name
}

output "cluster_endpoint" {
  description = "Endpoint del cluster DOKS"
  value       = module.doks.cluster_endpoint
  sensitive   = true
}

output "cluster_kubeconfig" {
  description = "Kubeconfig del cluster (para usar con kubectl)"
  value       = module.doks.kubeconfig_raw
  sensitive   = true
}

# -----------------------------------------------------------------------------
# Container Registry Outputs
# -----------------------------------------------------------------------------
output "registry_url" {
  description = "URL del Container Registry"
  value       = module.docr.registry_url
}

output "registry_name" {
  description = "Nombre del Container Registry"
  value       = module.docr.registry_name
}

# -----------------------------------------------------------------------------
# Database Outputs
# -----------------------------------------------------------------------------
output "database_host" {
  description = "Host de la base de datos PostgreSQL"
  value       = module.database.host
}

output "database_port" {
  description = "Puerto de la base de datos PostgreSQL"
  value       = module.database.port
}

output "database_connection_uri" {
  description = "URI de conexión a PostgreSQL"
  value       = module.database.connection_uri
  sensitive   = true
}

output "database_names" {
  description = "Nombres de las bases de datos creadas"
  value       = module.database.database_names
}

# -----------------------------------------------------------------------------
# Redis Outputs
# -----------------------------------------------------------------------------
output "redis_host" {
  description = "Host de Redis (SignalR Backplane)"
  value       = module.redis.private_host
}

output "redis_port" {
  description = "Puerto de Redis"
  value       = module.redis.port
}

output "redis_connection_string" {
  description = "Connection string de Redis para .NET"
  value       = module.redis.connection_string
  sensitive   = true
}

# -----------------------------------------------------------------------------
# Spaces Outputs
# -----------------------------------------------------------------------------
output "spaces_endpoint" {
  description = "Endpoint de DigitalOcean Spaces"
  value       = module.spaces.endpoint
}

output "spaces_bucket_name" {
  description = "Nombre del bucket de Spaces"
  value       = module.spaces.bucket_name
}

output "spaces_bucket_domain" {
  description = "Dominio del bucket de Spaces"
  value       = module.spaces.bucket_domain_name
}

# -----------------------------------------------------------------------------
# Ingress Outputs
# -----------------------------------------------------------------------------
output "ingress_load_balancer_ip" {
  description = "IP del Load Balancer del Ingress"
  value       = module.ingress.load_balancer_ip
}

# -----------------------------------------------------------------------------
# Helper Outputs
# -----------------------------------------------------------------------------
output "kubectl_connect_command" {
  description = "Comando para conectar kubectl al cluster"
  value       = "doctl kubernetes cluster kubeconfig save ${module.doks.cluster_id}"
}

output "docker_login_command" {
  description = "Comando para login de Docker al registry"
  value       = "doctl registry login"
}

output "deploy_images_command" {
  description = "Comando para construir y subir imágenes"
  value       = "docker build -t ${module.docr.registry_url}/identity-api:v1.0.0 -f Microservices/Identity/Api/Dockerfile . && docker push ${module.docr.registry_url}/identity-api:v1.0.0"
}

# -----------------------------------------------------------------------------
# Cloudflare Outputs
# -----------------------------------------------------------------------------
output "cloudflare_dns_record" {
  description = "Registro DNS creado en Cloudflare"
  sensitive   = true
  value       = local.is_cloudflare_ready && module.ingress.load_balancer_ip != "pending" ? {
    name    = cloudflare_record.api_endpoint[0].name
    type    = cloudflare_record.api_endpoint[0].type
    content = cloudflare_record.api_endpoint[0].content
    proxied = cloudflare_record.api_endpoint[0].proxied
  } : null
}

output "cloudflare_dns_hostname" {
  description = "Hostname completo del registro DNS"
  value       = local.is_cloudflare_ready && module.ingress.load_balancer_ip != "pending" ? cloudflare_record.api_endpoint[0].hostname : "Not configured or pending"
  sensitive   = true
}

