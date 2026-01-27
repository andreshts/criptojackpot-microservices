# =============================================================================
# DOKS (Kubernetes) Module - CryptoJackpot DigitalOcean Infrastructure
# =============================================================================

resource "digitalocean_kubernetes_cluster" "main" {
  name    = var.name
  region  = var.region
  version = var.version_k8s
  vpc_uuid = var.vpc_uuid

  # High Availability para producción
  ha = var.ha_enabled

  # Auto-upgrade para parches de seguridad
  auto_upgrade = var.auto_upgrade

  # Maintenance window
  maintenance_policy {
    start_time = var.maintenance_start_time
    day        = var.maintenance_day
  }

  # Node Pool principal
  node_pool {
    name       = var.node_pool_name
    size       = var.node_size
    node_count = var.auto_scale ? null : var.node_count
    auto_scale = var.auto_scale
    min_nodes  = var.auto_scale ? var.min_nodes : null
    max_nodes  = var.auto_scale ? var.max_nodes : null
    
    labels = {
      service  = "cryptojackpot"
      pool     = "workers"
    }

    tags = var.tags

    # Taint opcional para workloads específicos
    # taint {
    #   key    = "workload"
    #   value  = "api"
    #   effect = "NoSchedule"
    # }
  }

  tags = var.tags

  lifecycle {
    prevent_destroy = false
  }
}

# Obtener credenciales del cluster
data "digitalocean_kubernetes_cluster" "main" {
  name = digitalocean_kubernetes_cluster.main.name
  
  depends_on = [digitalocean_kubernetes_cluster.main]
}
