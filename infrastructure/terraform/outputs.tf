# =============================================================================
# Outputs - CriptoJackpot DigitalOcean Infrastructure
# =============================================================================

output "vpc_id" {
  description = "ID de la VPC"
  value       = module.vpc.vpc_id
}

# Kubernetes
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

# Container Registry
output "registry_url" {
  description = "URL del Container Registry"
  value       = module.docr.registry_url
}

output "registry_name" {
  description = "Nombre del Container Registry"
  value       = module.docr.registry_name
}

# Database
output "database_host" {
  description = "Host de la base de datos PostgreSQL (DO Managed)"
  value       = module.database.host
}

output "database_port" {
  description = "Puerto de la base de datos"
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

# Spaces
output "spaces_endpoint" {
  description = "Endpoint de DigitalOcean Spaces"
  value       = module.spaces.endpoint
}

output "spaces_bucket_name" {
  description = "Nombre del bucket de Spaces"
  value       = module.spaces.bucket_name
}

# Ingress
output "ingress_load_balancer_ip" {
  description = "IP del Load Balancer del NGINX Ingress (apuntar Cloudflare DNS a esta IP)"
  value       = module.ingress.load_balancer_ip
}

# Cloudflare
output "cloudflare_dns_record" {
  description = "Registro DNS creado en Cloudflare"
  value = local.is_cloudflare_ready && module.ingress.load_balancer_ip != "pending" ? {
    name    = cloudflare_record.api_endpoint[0].name
    type    = cloudflare_record.api_endpoint[0].type
    content = cloudflare_record.api_endpoint[0].content
    proxied = cloudflare_record.api_endpoint[0].proxied
    fqdn    = cloudflare_record.api_endpoint[0].hostname
  } : null
  sensitive = true
}

# =============================================================================
# Helper Commands
# =============================================================================
output "cmd_kubectl_connect" {
  description = "Comando para conectar kubectl al cluster"
  value       = "doctl kubernetes cluster kubeconfig save ${module.doks.cluster_id} --context ${local.resource_prefix}"
}

output "cmd_docker_login" {
  description = "Comando para login de Docker al registry"
  value       = "doctl registry login --expiry-seconds 3600"
}

output "cmd_kustomize_apply" {
  description = "Comando para desplegar los manifiestos Kubernetes"
  value       = "kubectl apply -k infrastructure/k8s/overlays/${var.environment} --context ${local.resource_prefix}"
}

output "cmd_build_push_images" {
  description = "Patrón de comando para construir y subir imágenes al registry"
  value       = "docker build -t ${module.docr.registry_url}/<service>:${var.environment} -f Microservices/<Service>/Api/Dockerfile . && docker push ${module.docr.registry_url}/<service>:${var.environment}"
}
