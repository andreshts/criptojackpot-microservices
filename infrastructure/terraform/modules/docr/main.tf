# =============================================================================
# DOCR (Container Registry) Module - CriptoJackpot DigitalOcean Infrastructure
# =============================================================================

resource "digitalocean_container_registry" "main" {
  name                   = var.name
  subscription_tier_slug = var.subscription_tier
  region                 = var.region
}

resource "digitalocean_container_registry_docker_credentials" "main" {
  registry_name = digitalocean_container_registry.main.name
}

# Integrar el registry con el cluster DOKS (permite pull de imágenes privadas)
resource "digitalocean_kubernetes_cluster_registry" "main" {
  count      = var.kubernetes_cluster_id != "" ? 1 : 0
  cluster_id = var.kubernetes_cluster_id
  registry_id = digitalocean_container_registry.main.id
}

