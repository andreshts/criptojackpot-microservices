# =============================================================================
# Ingress Module Outputs
# =============================================================================

output "load_balancer_ip" {
  description = "IP del Load Balancer del Ingress"
  value       = try(data.kubernetes_service.nginx_ingress.status[0].load_balancer[0].ingress[0].ip, "pending")
}

output "load_balancer_hostname" {
  description = "Hostname del Load Balancer del Ingress"
  value       = try(data.kubernetes_service.nginx_ingress.status[0].load_balancer[0].ingress[0].hostname, "pending")
}

output "nginx_ingress_namespace" {
  description = "Namespace del NGINX Ingress Controller"
  value       = helm_release.nginx_ingress.namespace
}

output "cert_manager_namespace" {
  description = "Namespace del Cert-Manager"
  value       = var.enable_ssl ? helm_release.cert_manager[0].namespace : null
}

