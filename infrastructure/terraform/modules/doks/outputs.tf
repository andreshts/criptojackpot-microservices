# =============================================================================
# DOKS Module Outputs
# =============================================================================

output "cluster_id" {
  description = "ID del cluster de Kubernetes"
  value       = digitalocean_kubernetes_cluster.main.id
}

output "cluster_name" {
  description = "Nombre del cluster"
  value       = digitalocean_kubernetes_cluster.main.name
}

output "cluster_endpoint" {
  description = "Endpoint del API server"
  value       = digitalocean_kubernetes_cluster.main.endpoint
  sensitive   = true
}

output "cluster_token" {
  description = "Token para autenticación"
  value       = data.digitalocean_kubernetes_cluster.main.kube_config[0].token
  sensitive   = true
}

output "cluster_ca_certificate" {
  description = "Certificado CA del cluster (base64)"
  value       = digitalocean_kubernetes_cluster.main.kube_config[0].cluster_ca_certificate
  sensitive   = true
}

output "kubeconfig_raw" {
  description = "Kubeconfig completo en formato YAML"
  value       = digitalocean_kubernetes_cluster.main.kube_config[0].raw_config
  sensitive   = true
}

output "cluster_urn" {
  description = "URN del cluster"
  value       = digitalocean_kubernetes_cluster.main.urn
}

output "cluster_status" {
  description = "Estado del cluster"
  value       = digitalocean_kubernetes_cluster.main.status
}

output "cluster_ipv4_address" {
  description = "Dirección IPv4 del cluster"
  value       = digitalocean_kubernetes_cluster.main.ipv4_address
}

output "node_pool_id" {
  description = "ID del node pool principal"
  value       = digitalocean_kubernetes_cluster.main.node_pool[0].id
}

