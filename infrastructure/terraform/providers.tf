# =============================================================================
# Terraform Providers - CryptoJackpot DigitalOcean Infrastructure
# =============================================================================

provider "digitalocean" {
  token             = var.do_token
  spaces_access_id  = var.spaces_access_key
  spaces_secret_key = var.spaces_secret_key
}

# Provider Kubernetes configurado después de crear el cluster DOKS
provider "kubernetes" {
  host                   = module.doks.cluster_endpoint
  token                  = module.doks.cluster_token
  cluster_ca_certificate = base64decode(module.doks.cluster_ca_certificate)
}

# Provider Helm para instalar cert-manager y nginx-ingress
provider "helm" {
  kubernetes {
    host                   = module.doks.cluster_endpoint
    token                  = module.doks.cluster_token
    cluster_ca_certificate = base64decode(module.doks.cluster_ca_certificate)
  }
}

# Provider Cloudflare para automatización DNS
provider "cloudflare" {
  api_token = var.cloudflare_api_token
}

