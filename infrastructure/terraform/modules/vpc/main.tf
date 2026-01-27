# =============================================================================
# VPC Module - CryptoJackpot DigitalOcean Infrastructure
# =============================================================================

resource "digitalocean_vpc" "main" {
  name        = var.name
  region      = var.region
  ip_range    = var.ip_range
  description = var.description
}
