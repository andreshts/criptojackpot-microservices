# =============================================================================
# VPC Module Outputs
# =============================================================================

output "vpc_id" {
  description = "ID de la VPC"
  value       = digitalocean_vpc.main.id
}

output "vpc_urn" {
  description = "URN de la VPC"
  value       = digitalocean_vpc.main.urn
}

output "ip_range" {
  description = "Rango de IP de la VPC"
  value       = digitalocean_vpc.main.ip_range
}

