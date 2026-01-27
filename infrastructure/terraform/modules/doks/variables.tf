# =============================================================================
# DOKS Module Variables
# =============================================================================

variable "name" {
  description = "Nombre del cluster de Kubernetes"
  type        = string
}

variable "region" {
  description = "Región de DigitalOcean"
  type        = string
}

variable "version_k8s" {
  description = "Versión de Kubernetes"
  type        = string
}

variable "vpc_uuid" {
  description = "UUID de la VPC donde desplegar el cluster"
  type        = string
}

variable "ha_enabled" {
  description = "Habilitar High Availability para el control plane"
  type        = bool
  default     = false
}

variable "auto_upgrade" {
  description = "Habilitar auto-upgrade de parches de seguridad"
  type        = bool
  default     = true
}

variable "maintenance_start_time" {
  description = "Hora de inicio de la ventana de mantenimiento (formato: HH:MM)"
  type        = string
  default     = "04:00"
}

variable "maintenance_day" {
  description = "Día de la semana para mantenimiento"
  type        = string
  default     = "sunday"
}

# Node Pool Configuration
variable "node_pool_name" {
  description = "Nombre del node pool principal"
  type        = string
}

variable "node_size" {
  description = "Tamaño de los nodos (droplet size)"
  type        = string
}

variable "node_count" {
  description = "Número de nodos (si auto_scale está deshabilitado)"
  type        = number
  default     = 3
}

variable "auto_scale" {
  description = "Habilitar auto-scaling del node pool"
  type        = bool
  default     = true
}

variable "min_nodes" {
  description = "Número mínimo de nodos (si auto_scale está habilitado)"
  type        = number
  default     = 2
}

variable "max_nodes" {
  description = "Número máximo de nodos (si auto_scale está habilitado)"
  type        = number
  default     = 5
}

variable "tags" {
  description = "Tags para el cluster"
  type        = list(string)
  default     = []
}

