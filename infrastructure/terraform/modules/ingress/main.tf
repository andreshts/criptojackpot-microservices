# =============================================================================
# Ingress Module - CriptoJackpot
# Instala NGINX Ingress Controller via Helm
# TLS es gestionado por Cloudflare externamente — cert-manager NO se instala
# en QA/prod. El ingress recibe HTTP plano desde Cloudflare (proxy ON).
# =============================================================================

resource "helm_release" "nginx_ingress" {
  name             = "ingress-nginx"
  repository       = "https://kubernetes.github.io/ingress-nginx"
  chart            = "ingress-nginx"
  namespace        = "ingress-nginx"
  create_namespace = true
  version          = var.nginx_ingress_version

  values = [
    <<-EOT
    controller:
      replicaCount: ${var.ingress_replicas}
      service:
        type: LoadBalancer
        annotations:
          service.beta.kubernetes.io/do-loadbalancer-name: "criptojackpot-lb"
          service.beta.kubernetes.io/do-loadbalancer-protocol: "http"
          service.beta.kubernetes.io/do-loadbalancer-algorithm: "round_robin"
          service.beta.kubernetes.io/do-loadbalancer-healthcheck-path: "/healthz"
          service.beta.kubernetes.io/do-loadbalancer-healthcheck-protocol: "http"
          # Pasar la IP real del cliente desde Cloudflare
          service.beta.kubernetes.io/do-loadbalancer-enable-proxy-protocol: "false"
      config:
        # Leer la IP real desde CF-Connecting-IP (seteado en el ingress patch de cada overlay)
        use-forwarded-headers: "true"
        forwarded-for-header: "CF-Connecting-IP"
      metrics:
        enabled: ${var.enable_metrics}
      resources:
        requests:
          cpu: 100m
          memory: 128Mi
        limits:
          cpu: 500m
          memory: 256Mi
    EOT
  ]

  wait    = true
  timeout = 600
}

# Obtener la IP del Load Balancer
data "kubernetes_service" "nginx_ingress" {
  metadata {
    name      = "ingress-nginx-controller"
    namespace = "ingress-nginx"
  }

  depends_on = [helm_release.nginx_ingress]
}
