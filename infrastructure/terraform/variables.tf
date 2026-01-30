# =============================================================================
# Variables - CryptoJackpot DigitalOcean Infrastructure
# =============================================================================

# -----------------------------------------------------------------------------
# DigitalOcean Authentication
# -----------------------------------------------------------------------------
variable "do_token" {
  description = "Token de API de DigitalOcean"
  type        = string
  sensitive   = true
}

variable "spaces_access_key" {
  description = "Access Key para DigitalOcean Spaces"
  type        = string
  sensitive   = true
}

variable "spaces_secret_key" {
  description = "Secret Key para DigitalOcean Spaces"
  type        = string
  sensitive   = true
}

# -----------------------------------------------------------------------------
# Project Configuration
# -----------------------------------------------------------------------------
variable "project_name" {
  description = "Nombre del proyecto"
  type        = string
  default     = "cryptojackpot"
}

variable "environment" {
  description = "Ambiente de despliegue (dev, staging, prod)"
  type        = string
  default     = "prod"

  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "El ambiente debe ser: dev, staging o prod."
  }
}

variable "region" {
  description = "Región de DigitalOcean"
  type        = string
  default     = "nyc3"
}

# -----------------------------------------------------------------------------
# VPC Configuration
# -----------------------------------------------------------------------------
variable "vpc_ip_range" {
  description = "Rango de IP para la VPC"
  type        = string
  default     = "10.10.0.0/16"
}

# -----------------------------------------------------------------------------
# Kubernetes (DOKS) Configuration
# -----------------------------------------------------------------------------
variable "k8s_version" {
  description = "Versión de Kubernetes para DOKS"
  type        = string
  default     = "1.29.1-do.0"
}

variable "k8s_node_size" {
  description = "Tamaño de los nodos del cluster"
  type        = string
  default     = "s-2vcpu-4gb"
}

variable "k8s_node_count" {
  description = "Número de nodos en el pool principal"
  type        = number
  default     = 3
}

variable "k8s_auto_scale" {
  description = "Habilitar auto-scaling"
  type        = bool
  default     = true
}

variable "k8s_min_nodes" {
  description = "Número mínimo de nodos (si auto-scaling está habilitado)"
  type        = number
  default     = 2
}

variable "k8s_max_nodes" {
  description = "Número máximo de nodos (si auto-scaling está habilitado)"
  type        = number
  default     = 5
}

# -----------------------------------------------------------------------------
# Database (PostgreSQL) Configuration
# -----------------------------------------------------------------------------
variable "db_size" {
  description = "Plan de la base de datos PostgreSQL"
  type        = string
  default     = "db-s-1vcpu-1gb"
}

variable "db_node_count" {
  description = "Número de nodos de la base de datos (1 = standalone, 2+ = HA)"
  type        = number
  default     = 1
}

variable "db_version" {
  description = "Versión de PostgreSQL"
  type        = string
  default     = "16"
}

# Lista de bases de datos a crear
variable "databases" {
  description = "Lista de bases de datos a crear"
  type        = list(string)
  default = [
    "cryptojackpot_identity_db",
    "cryptojackpot_lottery_db",
    "cryptojackpot_order_db",
    "cryptojackpot_wallet_db",
    "cryptojackpot_winner_db",
    "cryptojackpot_notification_db"
  ]
}

# -----------------------------------------------------------------------------
# Redis (SignalR Backplane) Configuration
# -----------------------------------------------------------------------------
variable "redis_size" {
  description = "Plan de Redis (db-s-1vcpu-1gb, db-s-1vcpu-2gb, etc.)"
  type        = string
  default     = "db-s-1vcpu-1gb"
}

variable "redis_node_count" {
  description = "Número de nodos de Redis (1 = standalone, 2+ = HA)"
  type        = number
  default     = 1
}

variable "redis_version" {
  description = "Versión de Redis"
  type        = string
  default     = "7"
}

# -----------------------------------------------------------------------------
# Container Registry (DOCR) Configuration
# -----------------------------------------------------------------------------
variable "registry_subscription_tier" {
  description = "Tier del Container Registry (starter, basic, professional)"
  type        = string
  default     = "basic"
}

# -----------------------------------------------------------------------------
# Spaces (Object Storage) Configuration
# -----------------------------------------------------------------------------
variable "spaces_bucket_name" {
  description = "Nombre del bucket de Spaces"
  type        = string
  default     = "cryptojackpot-assets"
}

variable "spaces_acl" {
  description = "ACL del bucket (private, public-read)"
  type        = string
  default     = "private"
}

variable "spaces_force_destroy" {
  description = <<-EOT
    ⚠️ PELIGROSO: Permitir destrucción del bucket aunque tenga objetos.
    NUNCA habilitar en producción - podría borrar todas las imágenes de usuarios.
    Solo usar en ambientes de desarrollo para limpieza rápida.
  EOT
  type        = bool
  default     = false
}

# -----------------------------------------------------------------------------
# Security Configuration
# -----------------------------------------------------------------------------
variable "jwt_secret_key" {
  description = "Clave secreta para JWT"
  type        = string
  sensitive   = true
  default     = "" # Se generará automáticamente si está vacío
}

variable "jwt_issuer" {
  description = "Issuer del JWT"
  type        = string
  default     = "CryptoJackpotIdentity"
}

variable "jwt_audience" {
  description = "Audience del JWT"
  type        = string
  default     = "CryptoJackpotApp"
}

variable "kafka_password" {
  description = "Contraseña para Kafka/Redpanda SASL"
  type        = string
  sensitive   = true
  default     = "" # Se generará automáticamente si está vacío
}

# -----------------------------------------------------------------------------
# Domain Configuration
# -----------------------------------------------------------------------------
variable "domain" {
  description = "Dominio principal para el ingress"
  type        = string
  default     = "api.cryptojackpot.com"
}

variable "enable_ssl" {
  description = "Habilitar SSL con Let's Encrypt"
  type        = bool
  default     = true
}

variable "letsencrypt_email" {
  description = "Email para Let's Encrypt"
  type        = string
  default     = "admin@cryptojackpot.com"
}

# -----------------------------------------------------------------------------
# Tags
# -----------------------------------------------------------------------------
variable "tags" {
  description = "Tags para todos los recursos"
  type        = list(string)
  default     = ["cryptojackpot", "microservices", "terraform-managed"]
}

# -----------------------------------------------------------------------------
# Cloudflare Configuration
# -----------------------------------------------------------------------------
variable "cloudflare_api_token" {
  description = "Token de API de Cloudflare con permisos de editar DNS"
  type        = string
  sensitive   = true
  default     = "" # Dejar vacío si no se usa Cloudflare
}

variable "cloudflare_zone_id" {
  description = "Zone ID del dominio en Cloudflare (ej. cryptojackpot.com)"
  type        = string
  sensitive   = true
  default     = "" # Dejar vacío si no se usa Cloudflare
}

variable "cloudflare_proxied" {
  description = "Si es true, activa el proxy de Cloudflare (nube naranja - CDN/WAF)"
  type        = bool
  default     = true
}

variable "enable_cloudflare_dns" {
  description = "Habilitar la creación automática de registros DNS en Cloudflare"
  type        = bool
  default     = false
}

