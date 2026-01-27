# =============================================================================
# Terraform Versions - CryptoJackpot DigitalOcean Infrastructure
# =============================================================================

terraform {
  required_version = ">= 1.5.0"

  required_providers {
    digitalocean = {
      source  = "digitalocean/digitalocean"
      version = "~> 2.34"
    }
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.25"
    }
    helm = {
      source  = "hashicorp/helm"
      version = "~> 2.12"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.6"
    }
    local = {
      source  = "hashicorp/local"
      version = "~> 2.4"
    }
    null = {
      source  = "hashicorp/null"
      version = "~> 3.2"
    }
    cloudflare = {
      source  = "cloudflare/cloudflare"
      version = "~> 4.0"
    }
  }

  # ===========================================================================
  # BACKEND REMOTO (RECOMENDADO PARA EQUIPOS Y CI/CD)
  # ===========================================================================
  # 
  # Por defecto, Terraform guarda el estado localmente en terraform.tfstate.
  # Esto funciona para desarrollo individual, pero tiene problemas:
  #   - El archivo puede perderse si se borra la máquina
  #   - No se puede trabajar en equipo sin conflictos
  #   - No funciona bien con CI/CD (GitHub Actions, etc.)
  #
  # PARA HABILITAR BACKEND REMOTO:
  # 
  # 1. Crear un Space en DigitalOcean para el estado:
  #    doctl spaces create cryptojackpot-terraform-state --region nyc3
  #
  # 2. Descomentar el bloque backend "s3" abajo
  #
  # 3. Configurar las credenciales como variables de entorno:
  #    $env:AWS_ACCESS_KEY_ID = "tu_spaces_access_key"
  #    $env:AWS_SECRET_ACCESS_KEY = "tu_spaces_secret_key"
  #
  # 4. Ejecutar: terraform init -migrate-state
  #
  # ===========================================================================
  
  # backend "s3" {
  #   # DigitalOcean Spaces usa el protocolo S3
  #   endpoint = "nyc3.digitaloceanspaces.com"
  #   bucket   = "cryptojackpot-terraform-state"
  #   key      = "prod/terraform.tfstate"  # Usar carpetas por ambiente
  #   region   = "us-east-1"               # Requerido por S3, ignorado por DO
  #   
  #   # Configuración específica para DigitalOcean Spaces
  #   skip_credentials_validation = true
  #   skip_metadata_api_check     = true
  #   skip_region_validation      = true
  #   skip_requesting_account_id  = true
  #   skip_s3_checksum            = true
  #   
  #   # Habilitar bloqueo de estado (previene conflictos en equipo)
  #   # Nota: Requiere DynamoDB o similar. DO Spaces no lo soporta nativamente.
  #   # Para equipos grandes, considerar Terraform Cloud o usar un lock externo.
  # }
}

# ===========================================================================
# NOTA SOBRE SEGURIDAD DEL ESTADO
# ===========================================================================
# 
# El archivo terraform.tfstate contiene información sensible:
#   - Contraseñas de base de datos
#   - Claves de API
#   - Endpoints privados
#
# NUNCA subir terraform.tfstate a Git (ya está en .gitignore)
# Si usas backend remoto, asegúrate de que el bucket sea PRIVADO.
# ===========================================================================

