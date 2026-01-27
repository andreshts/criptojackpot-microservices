# =============================================================================
# Database Module Variables
# =============================================================================

variable "name" {
  description = "Nombre del cluster de base de datos"
  type        = string
}

variable "region" {
  description = "Región de DigitalOcean"
  type        = string
}

variable "size" {
  description = "Tamaño del droplet para la base de datos"
  type        = string
  default     = "db-s-1vcpu-1gb"
}

variable "node_count" {
  description = "Número de nodos (1 = standalone, 2+ = HA con réplicas)"
  type        = number
  default     = 1
}

variable "version_pg" {
  description = "Versión de PostgreSQL"
  type        = string
  default     = "16"
}

variable "vpc_uuid" {
  description = "UUID de la VPC para conexión privada"
  type        = string
}

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

variable "create_app_user" {
  description = "Crear usuario de aplicación separado del admin"
  type        = bool
  default     = false
}

variable "app_user_name" {
  description = "Nombre del usuario de aplicación"
  type        = string
  default     = "cryptojackpot_app"
}

variable "trusted_sources_ids" {
  description = "Lista de IDs de recursos DO que pueden acceder (ej: cluster K8s ID)"
  type        = list(string)
  default     = []
}

variable "trusted_ips" {
  description = "Lista de IPs permitidas para acceso (para desarrollo)"
  type        = list(string)
  default     = []
}

variable "enable_connection_pool" {
  description = "Habilitar connection pooling (PgBouncer)"
  type        = bool
  default     = false
}

variable "connection_pool_size" {
  description = "Tamaño del connection pool por base de datos"
  type        = number
  default     = 10
}

variable "maintenance_day" {
  description = "Día de la semana para mantenimiento"
  type        = string
  default     = "sunday"
}

variable "maintenance_hour" {
  description = "Hora UTC para mantenimiento"
  type        = string
  default     = "04:00:00"
}

variable "tags" {
  description = "Tags para el cluster de base de datos"
  type        = list(string)
  default     = []
}

