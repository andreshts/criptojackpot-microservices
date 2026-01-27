# =============================================================================
# VPC Module Variables
# =============================================================================

variable "name" {
  description = "Nombre de la VPC"
  type        = string
}

variable "region" {
  description = "Región de DigitalOcean"
  type        = string
}

variable "ip_range" {
  description = "Rango de IP CIDR para la VPC"
  type        = string
}

variable "description" {
  description = "Descripción de la VPC"
  type        = string
  default     = "VPC managed by Terraform"
}

