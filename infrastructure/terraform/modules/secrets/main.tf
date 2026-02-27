# =============================================================================
# Kubernetes Secrets Module - CriptoJackpot
# Crea todos los secrets necesarios directamente en el cluster DOKS
# Los valores vienen de los servicios gestionados (DO Managed PG, Upstash, etc.)
# =============================================================================

resource "kubernetes_namespace" "main" {
  metadata {
    name = var.namespace
    labels = {
      name        = var.namespace
      environment = var.environment
    }
  }
}

# -----------------------------------------------------------------------------
# postgres-secrets
# Los microservicios conectan a PgBouncer (ClusterIP interno) que hace proxy
# al DO Managed PostgreSQL. SSL deshabilitado en la conexión interna al PgBouncer
# porque la comunicación es dentro del cluster (VPC privada de DO).
# -----------------------------------------------------------------------------
resource "kubernetes_secret" "postgres" {
  metadata {
    name      = "postgres-secrets"
    namespace = kubernetes_namespace.main.metadata[0].name
  }

  data = {
    POSTGRES_HOST     = var.postgres_host
    POSTGRES_PORT     = tostring(var.postgres_port)
    POSTGRES_USER     = var.postgres_user
    POSTGRES_PASSWORD = var.postgres_password
    POSTGRES_SSLMODE  = "require"

    IDENTITY_DB_CONNECTION     = "Host=pgbouncer.${var.namespace}.svc.cluster.local;Port=6432;Database=${var.namespace}_identity_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Disable"
    LOTTERY_DB_CONNECTION      = "Host=pgbouncer.${var.namespace}.svc.cluster.local;Port=6432;Database=${var.namespace}_lottery_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Disable"
    ORDER_DB_CONNECTION        = "Host=pgbouncer.${var.namespace}.svc.cluster.local;Port=6432;Database=${var.namespace}_order_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Disable"
    WALLET_DB_CONNECTION       = "Host=pgbouncer.${var.namespace}.svc.cluster.local;Port=6432;Database=${var.namespace}_wallet_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Disable"
    WINNER_DB_CONNECTION       = "Host=pgbouncer.${var.namespace}.svc.cluster.local;Port=6432;Database=${var.namespace}_winner_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Disable"
    NOTIFICATION_DB_CONNECTION = "Host=pgbouncer.${var.namespace}.svc.cluster.local;Port=6432;Database=${var.namespace}_notification_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Disable"
  }

  type = "Opaque"
}

# -----------------------------------------------------------------------------
# jwt-secrets
# -----------------------------------------------------------------------------
resource "kubernetes_secret" "jwt" {
  metadata {
    name      = "jwt-secrets"
    namespace = kubernetes_namespace.main.metadata[0].name
  }

  data = {
    JWT_SECRET_KEY = var.jwt_secret_key
    JWT_ISSUER     = var.jwt_issuer
    JWT_AUDIENCE   = var.jwt_audience
  }

  type = "Opaque"
}

# -----------------------------------------------------------------------------
# kafka-secrets — Upstash Kafka (SASL_SSL)
# -----------------------------------------------------------------------------
resource "kubernetes_secret" "kafka" {
  metadata {
    name      = "kafka-secrets"
    namespace = kubernetes_namespace.main.metadata[0].name
  }

  data = {
    KAFKA_BOOTSTRAP_SERVERS = var.kafka_bootstrap_servers
    KAFKA_SASL_USERNAME     = var.kafka_sasl_username
    KAFKA_SASL_PASSWORD     = var.kafka_sasl_password
    KAFKA_SASL_MECHANISM    = var.kafka_sasl_mechanism
    KAFKA_SECURITY_PROTOCOL = var.kafka_security_protocol
  }

  type = "Opaque"
}

# -----------------------------------------------------------------------------
# redis-secrets — Upstash Redis (TLS)
# -----------------------------------------------------------------------------
resource "kubernetes_secret" "redis" {
  metadata {
    name      = "redis-secrets"
    namespace = kubernetes_namespace.main.metadata[0].name
  }

  data = {
    REDIS_CONNECTION_STRING = var.redis_connection_string
  }

  type = "Opaque"
}

# -----------------------------------------------------------------------------
# mongodb-secrets — MongoDB Atlas
# -----------------------------------------------------------------------------
resource "kubernetes_secret" "mongodb" {
  metadata {
    name      = "mongodb-secrets"
    namespace = kubernetes_namespace.main.metadata[0].name
  }

  data = {
    MONGODB_AUDIT_CONNECTION = var.mongodb_connection_string
    MONGODB_AUDIT_DATABASE   = var.mongodb_audit_database
  }

  type = "Opaque"
}

# -----------------------------------------------------------------------------
# digitalocean-spaces-secrets
# -----------------------------------------------------------------------------
resource "kubernetes_secret" "spaces" {
  metadata {
    name      = "digitalocean-spaces-secrets"
    namespace = kubernetes_namespace.main.metadata[0].name
  }

  data = {
    SPACES_ENDPOINT   = var.spaces_endpoint
    SPACES_REGION     = var.spaces_region
    SPACES_BUCKET     = var.spaces_bucket
    SPACES_ACCESS_KEY = var.spaces_access_key
    SPACES_SECRET_KEY = var.spaces_secret_key
  }

  type = "Opaque"
}

# -----------------------------------------------------------------------------
# brevo-secrets — Brevo (Notification service)
# -----------------------------------------------------------------------------
resource "kubernetes_secret" "brevo" {
  metadata {
    name      = "brevo-secrets"
    namespace = kubernetes_namespace.main.metadata[0].name
  }

  data = {
    BREVO_API_KEY      = var.brevo_api_key
    BREVO_SENDER_EMAIL = var.brevo_sender_email
    BREVO_SENDER_NAME  = var.brevo_sender_name
    BREVO_BASE_URL     = var.brevo_base_url
  }

  type = "Opaque"
}

# Generar el archivo secrets.yaml para referencia/backup
# ⚠️ IMPORTANTE: Este archivo contiene credenciales sensibles
# Está incluido en .gitignore para evitar commits accidentales
locals {
  secrets_yaml_content = <<-EOT
# =============================================================================
# ARCHIVO GENERADO AUTOMÁTICAMENTE POR TERRAFORM
# ⚠️ CONTIENE CREDENCIALES SENSIBLES - NO SUBIR A GIT
# NO EDITAR MANUALMENTE - Los cambios se perderán
# Generado: ${timestamp()}
# =============================================================================
apiVersion: v1
kind: Secret
metadata:
  name: postgres-secrets
  namespace: ${var.namespace}
type: Opaque
stringData:
  POSTGRES_HOST: "${var.postgres_host}"
  POSTGRES_PORT: "${var.postgres_port}"
  POSTGRES_USER: "${var.postgres_user}"
  POSTGRES_PASSWORD: "${var.postgres_password}"
  POSTGRES_SSLMODE: "require"
  IDENTITY_DB_CONNECTION: "Host=pgbouncer.${var.namespace}.svc.cluster.local;Port=6432;Database=${var.namespace}_identity_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Disable"
  LOTTERY_DB_CONNECTION: "Host=pgbouncer.${var.namespace}.svc.cluster.local;Port=6432;Database=${var.namespace}_lottery_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Disable"
  ORDER_DB_CONNECTION: "Host=pgbouncer.${var.namespace}.svc.cluster.local;Port=6432;Database=${var.namespace}_order_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Disable"
  WALLET_DB_CONNECTION: "Host=pgbouncer.${var.namespace}.svc.cluster.local;Port=6432;Database=${var.namespace}_wallet_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Disable"
  WINNER_DB_CONNECTION: "Host=pgbouncer.${var.namespace}.svc.cluster.local;Port=6432;Database=${var.namespace}_winner_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Disable"
  NOTIFICATION_DB_CONNECTION: "Host=pgbouncer.${var.namespace}.svc.cluster.local;Port=6432;Database=${var.namespace}_notification_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Disable"
---
apiVersion: v1
kind: Secret
metadata:
  name: jwt-secrets
  namespace: ${var.namespace}
type: Opaque
stringData:
  JWT_SECRET_KEY: "${var.jwt_secret_key}"
  JWT_ISSUER: "${var.jwt_issuer}"
  JWT_AUDIENCE: "${var.jwt_audience}"
---
apiVersion: v1
kind: Secret
metadata:
  name: kafka-secrets
  namespace: ${var.namespace}
type: Opaque
stringData:
  KAFKA_BOOTSTRAP_SERVERS: "${var.kafka_bootstrap_servers}"
  KAFKA_SASL_USERNAME: "${var.kafka_sasl_username}"
  KAFKA_SASL_PASSWORD: "${var.kafka_sasl_password}"
  KAFKA_SASL_MECHANISM: "${var.kafka_sasl_mechanism}"
  KAFKA_SECURITY_PROTOCOL: "${var.kafka_security_protocol}"
---
apiVersion: v1
kind: Secret
metadata:
  name: redis-secrets
  namespace: ${var.namespace}
type: Opaque
stringData:
  REDIS_CONNECTION_STRING: "${var.redis_connection_string}"
---
apiVersion: v1
kind: Secret
metadata:
  name: mongodb-secrets
  namespace: ${var.namespace}
type: Opaque
stringData:
  MONGODB_AUDIT_CONNECTION: "${var.mongodb_connection_string}"
  MONGODB_AUDIT_DATABASE: "${var.mongodb_audit_database}"
---
apiVersion: v1
kind: Secret
metadata:
  name: digitalocean-spaces-secrets
  namespace: ${var.namespace}
type: Opaque
stringData:
  SPACES_ENDPOINT: "${var.spaces_endpoint}"
  SPACES_REGION: "${var.spaces_region}"
  SPACES_BUCKET: "${var.spaces_bucket}"
  SPACES_ACCESS_KEY: "${var.spaces_access_key}"
  SPACES_SECRET_KEY: "${var.spaces_secret_key}"
---
apiVersion: v1
kind: Secret
metadata:
  name: brevo-secrets
  namespace: ${var.namespace}
type: Opaque
stringData:
  BREVO_API_KEY: "${var.brevo_api_key}"
  BREVO_SENDER_EMAIL: "${var.brevo_sender_email}"
  BREVO_SENDER_NAME: "${var.brevo_sender_name}"
  BREVO_BASE_URL: "${var.brevo_base_url}"
EOT
}
