# =============================================================================
# Spaces Module Variables
# =============================================================================

variable "name" {
  description = "Nombre del bucket de Spaces"
  type        = string
}

variable "region" {
  description = "Región de DigitalOcean para Spaces"
  type        = string
}

variable "acl" {
  description = "ACL del bucket (private, public-read)"
  type        = string
  default     = "private"
}

variable "versioning_enabled" {
  description = "Habilitar versionado de objetos"
  type        = bool
  default     = true
}

variable "enable_lifecycle_rules" {
  description = "Habilitar reglas de ciclo de vida"
  type        = bool
  default     = false
}

variable "noncurrent_version_expiration_days" {
  description = "Días antes de eliminar versiones antiguas"
  type        = number
  default     = 30
}

variable "cors_allowed_origins" {
  description = "Orígenes permitidos para CORS"
  type        = list(string)
  default     = []
}

variable "force_destroy" {
  description = "Permitir destrucción del bucket aunque tenga objetos"
  type        = bool
  default     = false
}

variable "create_directories" {
  description = "Lista de directorios (prefijos) a crear"
  type        = list(string)
  default = [
    "profile-images",
    "lottery-images",
    "prize-images",
    "documents"
  ]
}

variable "enable_public_read_policy" {
  description = "Habilitar política de lectura pública para /public/*"
  type        = bool
  default     = false
}

variable "enable_cdn" {
  description = "Habilitar CDN para el bucket"
  type        = bool
  default     = false
}

variable "cdn_ttl" {
  description = "TTL del CDN en segundos"
  type        = number
  default     = 3600
}

variable "cdn_certificate_name" {
  description = "Nombre del certificado para CDN custom domain"
  type        = string
  default     = null
}

variable "cdn_custom_domain" {
  description = "Dominio personalizado para CDN"
  type        = string
  default     = null
}

