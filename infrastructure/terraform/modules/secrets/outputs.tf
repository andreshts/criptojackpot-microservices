# =============================================================================
# Secrets Module Outputs
# =============================================================================

output "namespace" {
  description = "Namespace donde se crearon los secrets"
  value       = kubernetes_namespace.main.metadata[0].name
}

output "postgres_secret_name" {
  description = "Nombre del secret de PostgreSQL"
  value       = kubernetes_secret.postgres.metadata[0].name
}

output "jwt_secret_name" {
  description = "Nombre del secret de JWT"
  value       = kubernetes_secret.jwt.metadata[0].name
}

output "kafka_secret_name" {
  description = "Nombre del secret de Kafka"
  value       = kubernetes_secret.kafka.metadata[0].name
}

output "redis_secret_name" {
  description = "Nombre del secret de Redis"
  value       = kubernetes_secret.redis.metadata[0].name
}

output "mongodb_secret_name" {
  description = "Nombre del secret de MongoDB"
  value       = kubernetes_secret.mongodb.metadata[0].name
}

output "spaces_secret_name" {
  description = "Nombre del secret de Spaces"
  value       = kubernetes_secret.spaces.metadata[0].name
}

output "brevo_secret_name" {
  description = "Nombre del secret de Brevo"
  value       = kubernetes_secret.brevo.metadata[0].name
}

output "secrets_yaml_content" {
  description = "Contenido del archivo secrets.yaml generado"
  value       = local.secrets_yaml_content
  sensitive   = true
}
