# =============================================================================
# Secrets Module Variables
# =============================================================================

variable "namespace" {
  description = "Namespace de Kubernetes para los secrets"
  type        = string
  default     = "cryptojackpot"
}

# PostgreSQL Configuration
variable "postgres_host" {
  description = "Host de PostgreSQL"
  type        = string
}

variable "postgres_port" {
  description = "Puerto de PostgreSQL"
  type        = number
  default     = 25060
}

variable "postgres_user" {
  description = "Usuario de PostgreSQL"
  type        = string
}

variable "postgres_password" {
  description = "Contraseña de PostgreSQL"
  type        = string
  sensitive   = true
}

variable "databases" {
  description = "Lista de bases de datos"
  type        = list(string)
}

# JWT Configuration
variable "jwt_secret_key" {
  description = "Clave secreta para JWT"
  type        = string
  sensitive   = true
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

# Kafka/Redpanda Configuration
variable "kafka_bootstrap_servers" {
  description = "Servidores bootstrap de Kafka"
  type        = string
}

variable "kafka_app_username" {
  description = "Username para aplicación en Kafka"
  type        = string
}

variable "kafka_app_password" {
  description = "Password para aplicación en Kafka"
  type        = string
  sensitive   = true
}

variable "redpanda_admin_password" {
  description = "Password para admin de Redpanda"
  type        = string
  sensitive   = true
}

# DigitalOcean Spaces Configuration
variable "spaces_endpoint" {
  description = "Endpoint de DigitalOcean Spaces"
  type        = string
}

variable "spaces_region" {
  description = "Región de Spaces"
  type        = string
}

variable "spaces_bucket" {
  description = "Nombre del bucket"
  type        = string
}

variable "spaces_access_key" {
  description = "Access Key de Spaces"
  type        = string
  sensitive   = true
}

variable "spaces_secret_key" {
  description = "Secret Key de Spaces"
  type        = string
  sensitive   = true
}

