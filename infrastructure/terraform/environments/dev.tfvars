# =============================================================================
# Development Environment Variables
# =============================================================================

# Project
project_name = "cryptojackpot"
environment  = "dev"
region       = "nyc3"

# VPC
vpc_ip_range = "10.20.0.0/16"

# Kubernetes - Configuración mínima para desarrollo
k8s_version    = "1.29.1-do.0"
k8s_node_size  = "s-2vcpu-2gb"
k8s_node_count = 2
k8s_auto_scale = false
k8s_min_nodes  = 1
k8s_max_nodes  = 3

# Database - Standalone para desarrollo
db_size       = "db-s-1vcpu-1gb"
db_node_count = 1
db_version    = "16"

# Registry
registry_subscription_tier = "starter"

# Spaces
spaces_bucket_name = "cryptojackpot-dev-assets"
spaces_acl         = "private"

# ⚠️ Solo en desarrollo: permite borrar bucket con contenido
# Útil para resetear el ambiente rápidamente
spaces_force_destroy = true

# SSL - Usar staging de Let's Encrypt para evitar rate limits
enable_ssl        = true
letsencrypt_email = "dev@cryptojackpot.com"

# Domain
domain = "dev-api.cryptojackpot.com"

# Tags
tags = ["cryptojackpot", "dev", "terraform-managed"]

# -----------------------------------------------------------------------------
# Cloudflare Configuration
# -----------------------------------------------------------------------------
# Habilitar para automatizar la creación de registros DNS
enable_cloudflare_dns = false  # Cambiar a true cuando tengas las credenciales

# Las credenciales sensibles se pasan por variable de entorno o directamente:
# cloudflare_api_token = "tu_token_aqui"  # TF_VAR_cloudflare_api_token
# cloudflare_zone_id   = "tu_zone_id"     # TF_VAR_cloudflare_zone_id

# Desactivar proxy para desarrollo (facilita debugging)
cloudflare_proxied = false

