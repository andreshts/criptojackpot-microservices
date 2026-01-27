# =============================================================================
# Production Environment Variables
# =============================================================================

# Project
project_name = "cryptojackpot"
environment  = "prod"
region       = "nyc3"

# VPC
vpc_ip_range = "10.10.0.0/16"

# Kubernetes - Configuración robusta para producción
k8s_version    = "1.29.1-do.0"
k8s_node_size  = "s-4vcpu-8gb"
k8s_node_count = 3
k8s_auto_scale = true
k8s_min_nodes  = 3
k8s_max_nodes  = 10

# Database - HA para producción
db_size       = "db-s-2vcpu-4gb"
db_node_count = 2  # Habilita replicación y failover
db_version    = "16"

# Registry
registry_subscription_tier = "professional"

# Spaces
spaces_bucket_name = "cryptojackpot-prod-assets"
spaces_acl         = "private"

# ⚠️ CRÍTICO: NUNCA cambiar a true en producción
# Esto protege las imágenes de usuarios contra borrado accidental
spaces_force_destroy = false

# SSL - Producción con Let's Encrypt
enable_ssl        = true
letsencrypt_email = "admin@cryptojackpot.com"

# Domain
domain = "api.cryptojackpot.com"

# Tags
tags = ["cryptojackpot", "prod", "terraform-managed", "critical"]

# -----------------------------------------------------------------------------
# Cloudflare Configuration
# -----------------------------------------------------------------------------
# Habilitar para automatizar la creación de registros DNS
enable_cloudflare_dns = false  # Cambiar a true cuando tengas las credenciales

# Las credenciales sensibles se pasan por variable de entorno o directamente:
# cloudflare_api_token = "tu_token_aqui"  # TF_VAR_cloudflare_api_token
# cloudflare_zone_id   = "tu_zone_id"     # TF_VAR_cloudflare_zone_id

# Activar proxy en producción para CDN/WAF (Nube Naranja)
cloudflare_proxied = true

