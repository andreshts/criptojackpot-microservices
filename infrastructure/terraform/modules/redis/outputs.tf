# =============================================================================
# Redis Module Outputs
# =============================================================================

output "id" {
  description = "The ID of the Redis cluster"
  value       = digitalocean_database_cluster.redis.id
}

output "host" {
  description = "The hostname of the Redis cluster"
  value       = digitalocean_database_cluster.redis.host
}

output "private_host" {
  description = "The private hostname of the Redis cluster (for VPC access)"
  value       = digitalocean_database_cluster.redis.private_host
}

output "port" {
  description = "The port of the Redis cluster"
  value       = digitalocean_database_cluster.redis.port
}

output "password" {
  description = "The password for the Redis cluster"
  value       = digitalocean_database_cluster.redis.password
  sensitive   = true
}

output "uri" {
  description = "The full connection URI for the Redis cluster"
  value       = digitalocean_database_cluster.redis.uri
  sensitive   = true
}

output "private_uri" {
  description = "The private connection URI for the Redis cluster (for VPC access)"
  value       = digitalocean_database_cluster.redis.private_uri
  sensitive   = true
}

output "connection_string" {
  description = "Connection string formatted for .NET applications (host:port,password=xxx)"
  value       = "${digitalocean_database_cluster.redis.private_host}:${digitalocean_database_cluster.redis.port},password=${digitalocean_database_cluster.redis.password},ssl=true,abortConnect=false"
  sensitive   = true
}
