# =============================================================================
# Ingress Module Variables
# =============================================================================

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

variable "nginx_ingress_version" {
  description = "Versión del chart de NGINX Ingress"
  type        = string
  default     = "4.9.0"
}

variable "cert_manager_version" {
  description = "Versión del chart de Cert-Manager"
  type        = string
  default     = "1.14.2"
}

variable "ingress_replicas" {
  description = "Número de réplicas del ingress controller"
  type        = number
  default     = 2
}

variable "enable_metrics" {
  description = "Habilitar métricas de Prometheus"
  type        = bool
  default     = true
}

