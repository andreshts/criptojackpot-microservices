# =============================================================================
# Ingress Module Variables
# =============================================================================

# Mantenido por compatibilidad — siempre false en QA/prod (TLS via Cloudflare)
variable "enable_ssl" {
  description = "Deprecado — TLS gestionado por Cloudflare. Mantener false."
  type        = bool
  default     = false
}

variable "letsencrypt_email" {
  description = "Deprecado — TLS gestionado por Cloudflare. Dejar vacío."
  type        = string
  default     = ""
}

variable "nginx_ingress_version" {
  description = "Versión del chart de NGINX Ingress"
  type        = string
  default     = "4.11.0"
}

variable "ingress_replicas" {
  description = "Número de réplicas del ingress controller (1 en QA, 2 en prod)"
  type        = number
  default     = 1
}

variable "enable_metrics" {
  description = "Habilitar métricas de Prometheus"
  type        = bool
  default     = true
}
