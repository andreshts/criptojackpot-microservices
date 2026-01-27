terraform {
  required_providers {
    digitalocean = {
      source  = "digitalocean/digitalocean"
      version = "~> 2.34"
    }
    null = {
      source  = "hashicorp/null"
      version = "~> 3.2"
    }
  }
}
