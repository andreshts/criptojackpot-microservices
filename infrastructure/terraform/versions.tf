# =============================================================================
# Terraform Versions - CriptoJackpot DigitalOcean Infrastructure
# =============================================================================

terraform {
  required_version = ">= 1.7.0"

  required_providers {
    digitalocean = {
      source  = "digitalocean/digitalocean"
      version = "~> 2.40"
    }
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.31"
    }
    helm = {
      source  = "hashicorp/helm"
      version = "~> 2.14"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.6"
    }
    local = {
      source  = "hashicorp/local"
      version = "~> 2.5"
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
  # BACKEND REMOTO - DigitalOcean Spaces (S3-compatible)
  # Estado separado por ambiente: qa/terraform.tfstate y prod/terraform.tfstate
  #
  # Setup inicial:
  #   1. Crear Space para el estado (una sola vez):
  #      doctl spaces create criptojackpot-tf-state --region nyc3
  #
  #   2. Configurar credenciales como variables de entorno:
  #      $env:AWS_ACCESS_KEY_ID     = "<spaces_access_key>"
  #      $env:AWS_SECRET_ACCESS_KEY = "<spaces_secret_key>"
  #
  #   3. Inicializar pasando la key del ambiente:
  #      terraform init -backend-config="key=qa/terraform.tfstate"
  #      terraform init -backend-config="key=prod/terraform.tfstate"
  #
  # En CI/CD (GitHub Actions), la key se pasa como variable:
  #   TF_BACKEND_KEY: "qa/terraform.tfstate"
  # ===========================================================================
  backend "s3" {
    endpoint = "nyc3.digitaloceanspaces.com"
    bucket   = "criptojackpot-tf-state"
    # key se pasa en tiempo de init: -backend-config="key=<env>/terraform.tfstate"
    key    = "terraform.tfstate"
    region = "us-east-1" # Requerido por protocolo S3, ignorado por DO

    skip_credentials_validation = true
    skip_metadata_api_check     = true
    skip_region_validation      = true
    skip_requesting_account_id  = true
    skip_s3_checksum            = true
  }
}

# ===========================================================================
# NOTA SOBRE SEGURIDAD DEL ESTADO
# El archivo terraform.tfstate contiene información sensible.
# NUNCA subir terraform.tfstate a Git (ya está en .gitignore).
# El bucket criptojackpot-tf-state debe ser PRIVADO.
# ===========================================================================

