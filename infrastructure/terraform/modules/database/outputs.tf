# =============================================================================
# Database Module Outputs
# =============================================================================

output "cluster_id" {
  description = "ID del cluster de base de datos"
  value       = digitalocean_database_cluster.main.id
}

output "cluster_urn" {
  description = "URN del cluster de base de datos"
  value       = digitalocean_database_cluster.main.urn
}

output "host" {
  description = "Host de la base de datos"
  value       = digitalocean_database_cluster.main.host
}

output "private_host" {
  description = "Host privado de la base de datos (dentro de la VPC)"
  value       = digitalocean_database_cluster.main.private_host
}

output "port" {
  description = "Puerto de la base de datos"
  value       = digitalocean_database_cluster.main.port
}

output "user" {
  description = "Usuario administrador"
  value       = digitalocean_database_cluster.main.user
}

output "password" {
  description = "Contraseña del usuario administrador"
  value       = digitalocean_database_cluster.main.password
  sensitive   = true
}

output "database" {
  description = "Nombre de la base de datos por defecto"
  value       = digitalocean_database_cluster.main.database
}

output "connection_uri" {
  description = "URI de conexión completa"
  value       = digitalocean_database_cluster.main.uri
  sensitive   = true
}

output "private_uri" {
  description = "URI de conexión privada (dentro de la VPC)"
  value       = digitalocean_database_cluster.main.private_uri
  sensitive   = true
}

output "database_names" {
  description = "Lista de nombres de bases de datos creadas"
  value       = [for db in digitalocean_database_db.databases : db.name]
}

# Connection strings formateados para .NET
output "connection_strings" {
  description = "Connection strings para cada microservicio (formato .NET)"
  value = {
    for db_name in var.databases : db_name => join("", [
      "Host=", digitalocean_database_cluster.main.private_host, ";",
      "Port=", digitalocean_database_cluster.main.port, ";",
      "Database=", db_name, ";",
      "Username=", digitalocean_database_cluster.main.user, ";",
      "Password=", digitalocean_database_cluster.main.password, ";",
      "SSL Mode=Require;Trust Server Certificate=true"
    ])
  }
  sensitive = true
}

# Mapping de microservicio a connection string
output "microservice_connection_strings" {
  description = "Connection strings mapeados por nombre de microservicio"
  value = {
    "IDENTITY_DB_CONNECTION"     = "Host=${digitalocean_database_cluster.main.private_host};Port=${digitalocean_database_cluster.main.port};Database=cryptojackpot_identity_db;Username=${digitalocean_database_cluster.main.user};Password=${digitalocean_database_cluster.main.password};SSL Mode=Require;Trust Server Certificate=true"
    "LOTTERY_DB_CONNECTION"      = "Host=${digitalocean_database_cluster.main.private_host};Port=${digitalocean_database_cluster.main.port};Database=cryptojackpot_lottery_db;Username=${digitalocean_database_cluster.main.user};Password=${digitalocean_database_cluster.main.password};SSL Mode=Require;Trust Server Certificate=true"
    "ORDER_DB_CONNECTION"        = "Host=${digitalocean_database_cluster.main.private_host};Port=${digitalocean_database_cluster.main.port};Database=cryptojackpot_order_db;Username=${digitalocean_database_cluster.main.user};Password=${digitalocean_database_cluster.main.password};SSL Mode=Require;Trust Server Certificate=true"
    "WALLET_DB_CONNECTION"       = "Host=${digitalocean_database_cluster.main.private_host};Port=${digitalocean_database_cluster.main.port};Database=cryptojackpot_wallet_db;Username=${digitalocean_database_cluster.main.user};Password=${digitalocean_database_cluster.main.password};SSL Mode=Require;Trust Server Certificate=true"
    "WINNER_DB_CONNECTION"       = "Host=${digitalocean_database_cluster.main.private_host};Port=${digitalocean_database_cluster.main.port};Database=cryptojackpot_winner_db;Username=${digitalocean_database_cluster.main.user};Password=${digitalocean_database_cluster.main.password};SSL Mode=Require;Trust Server Certificate=true"
    "NOTIFICATION_DB_CONNECTION" = "Host=${digitalocean_database_cluster.main.private_host};Port=${digitalocean_database_cluster.main.port};Database=cryptojackpot_notification_db;Username=${digitalocean_database_cluster.main.user};Password=${digitalocean_database_cluster.main.password};SSL Mode=Require;Trust Server Certificate=true"
  }
  sensitive = true
}

