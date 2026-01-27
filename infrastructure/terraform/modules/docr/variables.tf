# =============================================================================
# DOCR Module Variables
# =============================================================================

variable "name" {
  description = "Nombre del Container Registry"
  type        = string
}

variable "subscription_tier" {
  description = "Tier del subscription (starter, basic, professional)"
  type        = string
  default     = "basic"

  validation {
    condition     = contains(["starter", "basic", "professional"], var.subscription_tier)
    error_message = "El tier debe ser: starter, basic o professional."
  }
}

variable "region" {
  description = "Región del Container Registry"
  type        = string
}

variable "kubernetes_cluster_id" {
  description = "ID del cluster de Kubernetes para integración"
  type        = string
  default     = ""
}

