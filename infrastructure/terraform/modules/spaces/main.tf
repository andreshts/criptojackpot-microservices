# =============================================================================
# Spaces (Object Storage) Module - CryptoJackpot DigitalOcean Infrastructure
# =============================================================================

resource "digitalocean_spaces_bucket" "main" {
  name   = var.name
  region = var.region
  acl    = var.acl

  # Versionado para recuperación de archivos
  versioning {
    enabled = var.versioning_enabled
  }

  # Reglas de ciclo de vida (opcional)
  dynamic "lifecycle_rule" {
    for_each = var.enable_lifecycle_rules ? [1] : []
    content {
      id      = "cleanup-old-versions"
      enabled = true

      # Eliminar versiones antiguas después de X días
      noncurrent_version_expiration {
        days = var.noncurrent_version_expiration_days
      }

      # Eliminar uploads incompletos
      abort_incomplete_multipart_upload_days = 7
    }
  }

  # CORS Configuration
  dynamic "cors_rule" {
    for_each = length(var.cors_allowed_origins) > 0 ? [1] : []
    content {
      allowed_headers = ["*"]
      allowed_methods = ["GET", "PUT", "POST", "DELETE", "HEAD"]
      allowed_origins = var.cors_allowed_origins
      max_age_seconds = 3600
    }
  }

  # Forzar destrucción del bucket (solo para dev)
  force_destroy = var.force_destroy
}

# Crear directorios (prefijos) para organización
resource "digitalocean_spaces_bucket_object" "directories" {
  for_each = toset(var.create_directories)

  region       = var.region
  bucket       = digitalocean_spaces_bucket.main.name
  key          = "${each.value}/.keep"
  content      = ""
  content_type = "application/x-directory"
  acl          = "private"
}

# Política del bucket (opcional - para acceso público a ciertos prefijos)
resource "digitalocean_spaces_bucket_policy" "public_read" {
  count = var.enable_public_read_policy ? 1 : 0

  region = var.region
  bucket = digitalocean_spaces_bucket.main.name

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid       = "PublicReadGetObject"
        Effect    = "Allow"
        Principal = "*"
        Action    = ["s3:GetObject"]
        Resource  = ["arn:aws:s3:::${digitalocean_spaces_bucket.main.name}/public/*"]
      }
    ]
  })
}

# CDN para el bucket (opcional - mejor rendimiento)
resource "digitalocean_cdn" "spaces_cdn" {
  count = var.enable_cdn ? 1 : 0

  origin           = digitalocean_spaces_bucket.main.bucket_domain_name
  ttl              = var.cdn_ttl
  certificate_name = var.cdn_certificate_name
  custom_domain    = var.cdn_custom_domain
}
