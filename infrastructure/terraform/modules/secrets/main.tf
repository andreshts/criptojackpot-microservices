# =============================================================================
# Kubernetes Secrets Module - CryptoJackpot DigitalOcean Infrastructure
# Genera automáticamente los secrets de Kubernetes con valores reales
# =============================================================================

# Crear el namespace si no existe
resource "kubernetes_namespace" "main" {
  metadata {
    name = var.namespace
    labels = {
      name = var.namespace
    }
  }
}

# Secret para PostgreSQL
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

    # Connection strings para cada microservicio
    # Services using PgBouncer connect via pgbouncer.cryptojackpot.svc.cluster.local:6432
    # PgBouncer handles connection pooling to the actual PostgreSQL server
    IDENTITY_DB_CONNECTION     = "Host=pgbouncer.cryptojackpot.svc.cluster.local;Port=6432;Database=cryptojackpot_identity_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Disable"
    LOTTERY_DB_CONNECTION      = "Host=${var.postgres_host};Port=${var.postgres_port};Database=cryptojackpot_lottery_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Require;Trust Server Certificate=true;MaxPoolSize=50;MinPoolSize=10;ConnectionLifetime=300"
    ORDER_DB_CONNECTION        = "Host=${var.postgres_host};Port=${var.postgres_port};Database=cryptojackpot_order_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Require;Trust Server Certificate=true;MaxPoolSize=50;MinPoolSize=10;ConnectionLifetime=300"
    WALLET_DB_CONNECTION       = "Host=${var.postgres_host};Port=${var.postgres_port};Database=cryptojackpot_wallet_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Require;Trust Server Certificate=true"
    WINNER_DB_CONNECTION       = "Host=${var.postgres_host};Port=${var.postgres_port};Database=cryptojackpot_winner_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Require;Trust Server Certificate=true"
    NOTIFICATION_DB_CONNECTION = "Host=${var.postgres_host};Port=${var.postgres_port};Database=cryptojackpot_notification_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Require;Trust Server Certificate=true"
  }

  type = "Opaque"
}

# Secret para JWT
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

# Secret para DigitalOcean Spaces
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

# Secret para Kafka/Redpanda
# NOTA: El servicio Redpanda debe estar desplegado en:
#   - Namespace: cryptojackpot (mismo que project_name)
#   - Service name: redpanda
#   - Port: 9092
# Esto genera el endpoint: redpanda.cryptojackpot.svc.cluster.local:9092
resource "kubernetes_secret" "kafka" {
  metadata {
    name      = "kafka-secrets"
    namespace = kubernetes_namespace.main.metadata[0].name
  }

  data = {
    KAFKA_BOOTSTRAP_SERVERS = var.kafka_bootstrap_servers
    KAFKA_SASL_USERNAME     = var.kafka_app_username
    KAFKA_SASL_PASSWORD     = var.kafka_app_password
    KAFKA_SASL_MECHANISM    = "SCRAM-SHA-256"
    KAFKA_SECURITY_PROTOCOL = "SASL_PLAINTEXT"
  }

  type = "Opaque"
}

# Secret para Redpanda credentials (admin y app)
resource "kubernetes_secret" "redpanda_credentials" {
  metadata {
    name      = "redpanda-credentials"
    namespace = kubernetes_namespace.main.metadata[0].name
  }

  data = {
    REDPANDA_SASL_USERNAME  = var.kafka_app_username
    REDPANDA_SASL_PASSWORD  = var.kafka_app_password
    REDPANDA_ADMIN_USERNAME = "admin"
    REDPANDA_ADMIN_PASSWORD = var.redpanda_admin_password
  }

  type = "Opaque"
}

# Secret para Redis (SignalR Backplane)
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
  IDENTITY_DB_CONNECTION: "Host=pgbouncer.cryptojackpot.svc.cluster.local;Port=6432;Database=cryptojackpot_identity_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Disable"
  LOTTERY_DB_CONNECTION: "Host=${var.postgres_host};Port=${var.postgres_port};Database=cryptojackpot_lottery_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Require;Trust Server Certificate=true;MaxPoolSize=50;MinPoolSize=10;ConnectionLifetime=300"
  ORDER_DB_CONNECTION: "Host=${var.postgres_host};Port=${var.postgres_port};Database=cryptojackpot_order_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Require;Trust Server Certificate=true;MaxPoolSize=50;MinPoolSize=10;ConnectionLifetime=300"
  WALLET_DB_CONNECTION: "Host=${var.postgres_host};Port=${var.postgres_port};Database=cryptojackpot_wallet_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Require;Trust Server Certificate=true"
  WINNER_DB_CONNECTION: "Host=${var.postgres_host};Port=${var.postgres_port};Database=cryptojackpot_winner_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Require;Trust Server Certificate=true"
  NOTIFICATION_DB_CONNECTION: "Host=${var.postgres_host};Port=${var.postgres_port};Database=cryptojackpot_notification_db;Username=${var.postgres_user};Password=${var.postgres_password};SSL Mode=Require;Trust Server Certificate=true"
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
  name: kafka-secrets
  namespace: ${var.namespace}
type: Opaque
stringData:
  KAFKA_BOOTSTRAP_SERVERS: "${var.kafka_bootstrap_servers}"
  KAFKA_SASL_USERNAME: "${var.kafka_app_username}"
  KAFKA_SASL_PASSWORD: "${var.kafka_app_password}"
  KAFKA_SASL_MECHANISM: "SCRAM-SHA-256"
  KAFKA_SECURITY_PROTOCOL: "SASL_PLAINTEXT"
---
apiVersion: v1
kind: Secret
metadata:
  name: redpanda-credentials
  namespace: ${var.namespace}
type: Opaque
stringData:
  REDPANDA_SASL_USERNAME: "${var.kafka_app_username}"
  REDPANDA_SASL_PASSWORD: "${var.kafka_app_password}"
  REDPANDA_ADMIN_USERNAME: "admin"
  REDPANDA_ADMIN_PASSWORD: "${var.redpanda_admin_password}"
---
apiVersion: v1
kind: Secret
metadata:
  name: redis-secrets
  namespace: ${var.namespace}
type: Opaque
stringData:
  REDIS_CONNECTION_STRING: "${var.redis_connection_string}"
EOT
}

