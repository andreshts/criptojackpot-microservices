# =============================================================================
# Variables - CriptoJackpot DigitalOcean Infrastructure
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
  default     = "criptojackpot"
}

variable "environment" {
  description = "Ambiente de despliegue (qa, prod)"
  type        = string
  default     = "prod"

  validation {
    condition     = contains(["qa", "prod"], var.environment)
    error_message = "El ambiente debe ser: qa o prod."
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
  default     = "1.32.2-do.0"
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
  default     = "db-s-1vcpu-2gb"
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
  description = "Lista de bases de datos a crear en DO Managed PostgreSQL"
  type        = list(string)
  default = [
    "criptojackpot_identity_db",
    "criptojackpot_lottery_db",
    "criptojackpot_order_db",
    "criptojackpot_wallet_db",
    "criptojackpot_winner_db",
    "criptojackpot_notification_db"
  ]
}

# -----------------------------------------------------------------------------
# Redis (Upstash - externo) Configuration
# -----------------------------------------------------------------------------
variable "redis_connection_string" {
  description = "Connection string de Upstash Redis (con TLS)"
  type        = string
  sensitive   = true
  default     = ""
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
  default     = "criptojackpot-assets"
}

variable "spaces_acl" {
  description = "ACL del bucket (private, public-read)"
  type        = string
  default     = "private"
}

variable "spaces_force_destroy" {
  description = "⚠️ Permitir destrucción del bucket aunque tenga objetos. NUNCA true en prod."
  type        = bool
  default     = false
}

# -----------------------------------------------------------------------------
# Security - JWT
# -----------------------------------------------------------------------------
variable "jwt_secret_key" {
  description = "Clave secreta para JWT (se autogenera si está vacío)"
  type        = string
  sensitive   = true
  default     = ""
}

variable "jwt_issuer" {
  description = "Issuer del JWT"
  type        = string
  default     = "CriptoJackpotIdentity"
}

variable "jwt_audience" {
  description = "Audience del JWT"
  type        = string
  default     = "CriptoJackpotApp"
}

# -----------------------------------------------------------------------------
# Kafka - Upstash (externo, SASL_SSL)
# -----------------------------------------------------------------------------
variable "kafka_bootstrap_servers" {
  description = "Bootstrap servers de Upstash Kafka (ej: host:9092)"
  type        = string
  sensitive   = true
  default     = ""
}

variable "kafka_sasl_username" {
  description = "Username SASL para Upstash Kafka"
  type        = string
  sensitive   = true
  default     = ""
}

variable "kafka_sasl_password" {
  description = "Password SASL para Upstash Kafka"
  type        = string
  sensitive   = true
  default     = ""
}

variable "kafka_sasl_mechanism" {
  description = "Mecanismo SASL para Kafka (SCRAM-SHA-256)"
  type        = string
  default     = "SCRAM-SHA-256"
}

variable "kafka_security_protocol" {
  description = "Protocolo de seguridad Kafka (SASL_SSL para Upstash)"
  type        = string
  default     = "SASL_SSL"
}

# -----------------------------------------------------------------------------
# MongoDB Atlas (externo) - Audit Service
# -----------------------------------------------------------------------------
variable "mongodb_connection_string" {
  description = "Connection string de MongoDB Atlas (mongodb+srv://...)"
  type        = string
  sensitive   = true
  default     = ""
}

variable "mongodb_audit_database" {
  description = "Nombre de la base de datos de auditoría en MongoDB Atlas"
  type        = string
  default     = "criptojackpot_audit"
}

# -----------------------------------------------------------------------------
# Brevo (Email) - Notification Service
# -----------------------------------------------------------------------------
variable "brevo_api_key" {
  description = "API Key de Brevo para envío de emails"
  type        = string
  sensitive   = true
  default     = ""
}

variable "brevo_sender_email" {
  description = "Email remitente en Brevo"
  type        = string
  default     = "noreply@criptojackpot.com"
}

variable "brevo_sender_name" {
  description = "Nombre del remitente en Brevo"
  type        = string
  default     = "CriptoJackpot"
}

# -----------------------------------------------------------------------------
# Domain Configuration
# -----------------------------------------------------------------------------
variable "domain" {
  description = "Dominio del BFF (punto de entrada externo via Cloudflare)"
  type        = string
  default     = "api.criptojackpot.com"
}

# -----------------------------------------------------------------------------
# Tags
# -----------------------------------------------------------------------------
variable "tags" {
  description = "Tags para todos los recursos"
  type        = list(string)
  default     = ["criptojackpot", "microservices", "terraform-managed"]
}

# -----------------------------------------------------------------------------
# Cloudflare Configuration
# -----------------------------------------------------------------------------
variable "cloudflare_api_token" {
  description = "Token de API de Cloudflare con permisos de editar DNS"
  type        = string
  sensitive   = true
  default     = ""
}

variable "cloudflare_zone_id" {
  description = "Zone ID del dominio criptojackpot.com en Cloudflare"
  type        = string
  sensitive   = true
  default     = ""
}

variable "cloudflare_proxied" {
  description = "Activar proxy de Cloudflare (nube naranja - CDN/WAF)"
  type        = bool
  default     = true
}

variable "enable_cloudflare_dns" {
  description = "Crear registro DNS en Cloudflare automáticamente"
  type        = bool
  default     = true
}

