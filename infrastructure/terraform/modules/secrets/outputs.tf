# =============================================================================
# Secrets Module Outputs
# =============================================================================

output "postgres_secret_name" {
  description = "Nombre del secret de PostgreSQL"
  value       = kubernetes_secret.postgres.metadata[0].name
}

output "jwt_secret_name" {
  description = "Nombre del secret de JWT"
  value       = kubernetes_secret.jwt.metadata[0].name
}

output "spaces_secret_name" {
  description = "Nombre del secret de Spaces"
  value       = kubernetes_secret.spaces.metadata[0].name
}

output "kafka_secret_name" {
  description = "Nombre del secret de Kafka"
  value       = kubernetes_secret.kafka.metadata[0].name
}

output "redpanda_credentials_secret_name" {
  description = "Nombre del secret de Redpanda credentials"
  value       = kubernetes_secret.redpanda_credentials.metadata[0].name
}

output "namespace" {
  description = "Namespace donde se crearon los secrets"
  value       = kubernetes_namespace.main.metadata[0].name
}

output "secrets_yaml_content" {
  description = "Contenido del archivo secrets.yaml generado"
  value       = local.secrets_yaml_content
  sensitive   = true
}

