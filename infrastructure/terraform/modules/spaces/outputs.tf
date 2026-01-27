# =============================================================================
# Spaces Module Outputs
# =============================================================================

output "bucket_name" {
  description = "Nombre del bucket"
  value       = digitalocean_spaces_bucket.main.name
}

output "bucket_urn" {
  description = "URN del bucket"
  value       = digitalocean_spaces_bucket.main.urn
}

output "bucket_domain_name" {
  description = "Dominio del bucket"
  value       = digitalocean_spaces_bucket.main.bucket_domain_name
}

output "endpoint" {
  description = "Endpoint del bucket (para SDK)"
  value       = "https://${var.region}.digitaloceanspaces.com"
}

output "bucket_regional_domain" {
  description = "Dominio regional del bucket"
  value       = "${digitalocean_spaces_bucket.main.name}.${var.region}.digitaloceanspaces.com"
}

output "cdn_domain" {
  description = "Dominio del CDN (si está habilitado)"
  value       = var.enable_cdn ? digitalocean_cdn.spaces_cdn[0].endpoint : null
}

output "cdn_id" {
  description = "ID del CDN (si está habilitado)"
  value       = var.enable_cdn ? digitalocean_cdn.spaces_cdn[0].id : null
}

