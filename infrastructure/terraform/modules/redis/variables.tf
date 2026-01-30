# =============================================================================
# Redis Module Variables
# =============================================================================

variable "name" {
  description = "Name of the Redis cluster"
  type        = string
}

variable "region" {
  description = "DigitalOcean region"
  type        = string
}

variable "size" {
  description = "Database droplet size (db-s-1vcpu-1gb, db-s-1vcpu-2gb, etc.)"
  type        = string
  default     = "db-s-1vcpu-1gb"
}

variable "node_count" {
  description = "Number of nodes in the cluster"
  type        = number
  default     = 1
}

variable "version_redis" {
  description = "Redis version"
  type        = string
  default     = "7"
}

variable "vpc_uuid" {
  description = "VPC UUID for private networking"
  type        = string
}

variable "tags" {
  description = "Tags to apply to the cluster"
  type        = list(string)
  default     = []
}

variable "maintenance_day" {
  description = "Day of the week for maintenance (monday, tuesday, etc.)"
  type        = string
  default     = "sunday"
}

variable "maintenance_hour" {
  description = "Hour of the day for maintenance (UTC, 00-23)"
  type        = string
  default     = "03:00"
}

variable "eviction_policy" {
  description = "Redis eviction policy (noeviction, allkeys_lru, allkeys_random, volatile_lru, volatile_random, volatile_ttl)"
  type        = string
  default     = "allkeys_lru"
}

variable "trusted_sources_ids" {
  description = "List of Kubernetes cluster IDs that can access Redis"
  type        = list(string)
  default     = []
}

variable "trusted_ips" {
  description = "List of IP addresses that can access Redis (for development)"
  type        = list(string)
  default     = []
}
