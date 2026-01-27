# =============================================================================
# DOCR (Container Registry) Module - CryptoJackpot DigitalOcean Infrastructure
# =============================================================================

resource "digitalocean_container_registry" "main" {
  name                   = var.name
  subscription_tier_slug = var.subscription_tier
  region                 = var.region
}

# Integración automática del registry con el cluster de Kubernetes
resource "digitalocean_container_registry_docker_credentials" "main" {
  registry_name = digitalocean_container_registry.main.name
}


# Crear secret de Docker Registry en Kubernetes
resource "null_resource" "registry_secret" {
  count = var.kubernetes_cluster_id != "" ? 1 : 0

  provisioner "local-exec" {
    command = <<-EOT
      kubectl create secret docker-registry do-registry \
        --docker-server=${digitalocean_container_registry.main.server_url} \
        --docker-username=${digitalocean_container_registry_docker_credentials.main.docker_credentials} \
        --docker-password=${digitalocean_container_registry_docker_credentials.main.docker_credentials} \
        --namespace=cryptojackpot \
        --dry-run=client -o yaml | kubectl apply -f -
    EOT
  }

  depends_on = [digitalocean_container_registry.main]
}
