# =============================================================================
# Database (PostgreSQL) Module - CryptoJackpot DigitalOcean Infrastructure
# =============================================================================

resource "digitalocean_database_cluster" "main" {
  name       = var.name
  engine     = "pg"
  version    = var.version_pg
  size       = var.size
  region     = var.region
  node_count = var.node_count
  
  # Conexión a VPC privada
  private_network_uuid = var.vpc_uuid

  # Tags para identificación
  tags = var.tags

  # Configuraciones de mantenimiento
  maintenance_window {
    day  = var.maintenance_day
    hour = var.maintenance_hour
  }

  lifecycle {
    prevent_destroy = false
  }
}

# Crear las 6 bases de datos para cada microservicio
resource "digitalocean_database_db" "databases" {
  for_each = toset(var.databases)

  cluster_id = digitalocean_database_cluster.main.id
  name       = each.value
}

# Usuario de aplicación (opcional - usar si se quiere separar del admin)
resource "digitalocean_database_user" "app_user" {
  count = var.create_app_user ? 1 : 0

  cluster_id = digitalocean_database_cluster.main.id
  name       = var.app_user_name
}

# Firewall de base de datos - Solo permitir acceso desde el cluster K8s
resource "digitalocean_database_firewall" "main" {
  cluster_id = digitalocean_database_cluster.main.id

  # Permitir acceso desde recursos específicos de DO (cluster K8s)
  dynamic "rule" {
    for_each = var.trusted_sources_ids
    content {
      type  = "k8s"
      value = rule.value
    }
  }

  # Opcionalmente permitir IPs específicas (para desarrollo)
  dynamic "rule" {
    for_each = var.trusted_ips
    content {
      type  = "ip_addr"
      value = rule.value
    }
  }
}

# Configuración de conexión pool (para mejor rendimiento)
resource "digitalocean_database_connection_pool" "main" {
  for_each = var.enable_connection_pool ? toset(var.databases) : []

  cluster_id = digitalocean_database_cluster.main.id
  name       = "${each.value}-pool"
  mode       = "transaction"
  size       = var.connection_pool_size
  db_name    = each.value
  user       = digitalocean_database_cluster.main.user

  depends_on = [digitalocean_database_db.databases]
}
