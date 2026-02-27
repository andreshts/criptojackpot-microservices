# =============================================================================
# QA Environment Variables - CriptoJackpot
# =============================================================================
# Uso:
#   terraform init -backend-config="key=qa/terraform.tfstate"
#   terraform apply -var-file="environments/qa.tfvars"
#
# Secrets sensibles via variables de entorno:
#   $env:TF_VAR_do_token              = "dop_v1_..."
#   $env:TF_VAR_spaces_access_key     = "..."
#   $env:TF_VAR_spaces_secret_key     = "..."
#   $env:TF_VAR_cloudflare_api_token  = "..."
#   $env:TF_VAR_cloudflare_zone_id    = "..."
#   $env:TF_VAR_kafka_bootstrap_servers = "upstash-host:9092"
#   $env:TF_VAR_kafka_sasl_username   = "..."
#   $env:TF_VAR_kafka_sasl_password   = "..."
#   $env:TF_VAR_mongodb_connection_string = "mongodb+srv://..."
#   $env:TF_VAR_brevo_api_key         = "..."
# =============================================================================

# Project
project_name = "criptojackpot"
environment  = "qa"
region       = "nyc3"

# VPC (rango separado de prod para evitar overlaps)
vpc_ip_range = "10.30.0.0/16"

# Kubernetes - 2 nodos fijos, tamaño medio
k8s_version    = "1.32.2-do.0"
k8s_node_size  = "s-2vcpu-4gb"
k8s_node_count = 2
k8s_auto_scale = false
k8s_min_nodes  = 2
k8s_max_nodes  = 4

# Database - Standalone para QA (sin HA)
db_size       = "db-s-1vcpu-2gb"
db_node_count = 1
db_version    = "16"

# Registry - compartido con prod (básico alcanza)
registry_subscription_tier = "basic"

# Spaces - bucket separado para QA
spaces_bucket_name   = "criptojackpot-qa-assets"
spaces_acl           = "private"
spaces_force_destroy = true  # OK en QA: permite limpiar el ambiente

# Domain - subdominio qa
domain = "api-qa.criptojackpot.com"

# Cloudflare (TLS terminado en CF, no se usa cert-manager)
enable_cloudflare_dns = true
cloudflare_proxied    = true   # Nube naranja: CDN + WAF activo en QA también

# JWT
jwt_issuer   = "CriptoJackpotIdentity"
jwt_audience = "CriptoJackpotApp"

# Kafka - Upstash (externo, SASL_SSL)
# Los valores reales se pasan via TF_VAR_* en CI/CD
kafka_sasl_mechanism    = "SCRAM-SHA-256"
kafka_security_protocol = "SASL_SSL"

# Tags
tags = ["criptojackpot", "qa", "terraform-managed"]

