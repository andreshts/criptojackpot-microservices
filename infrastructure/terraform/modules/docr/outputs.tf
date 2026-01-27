# =============================================================================
# DOCR Module Outputs
# =============================================================================

output "registry_id" {
  description = "ID del Container Registry"
  value       = digitalocean_container_registry.main.id
}

output "registry_name" {
  description = "Nombre del Container Registry"
  value       = digitalocean_container_registry.main.name
}

output "registry_url" {
  description = "URL del Container Registry (para docker push/pull)"
  value       = "registry.digitalocean.com/${digitalocean_container_registry.main.name}"
}

output "registry_server_url" {
  description = "URL del servidor del Registry"
  value       = digitalocean_container_registry.main.server_url
}

output "registry_endpoint" {
  description = "Endpoint del Registry"
  value       = digitalocean_container_registry.main.endpoint
}

output "docker_credentials" {
  description = "Credenciales Docker para el Registry"
  value       = digitalocean_container_registry_docker_credentials.main.docker_credentials
  sensitive   = true
}

