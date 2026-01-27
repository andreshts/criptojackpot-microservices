# =============================================================================
# Ingress Module - CryptoJackpot DigitalOcean Infrastructure
# Instala NGINX Ingress Controller y Cert-Manager via Helm
# =============================================================================

# NGINX Ingress Controller
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
          service.beta.kubernetes.io/do-loadbalancer-name: "cryptojackpot-lb"
          service.beta.kubernetes.io/do-loadbalancer-protocol: "http"
          service.beta.kubernetes.io/do-loadbalancer-algorithm: "round_robin"
          service.beta.kubernetes.io/do-loadbalancer-healthcheck-path: "/healthz"
          service.beta.kubernetes.io/do-loadbalancer-healthcheck-protocol: "http"
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

# Cert-Manager para SSL/TLS automático
resource "helm_release" "cert_manager" {
  count = var.enable_ssl ? 1 : 0

  name             = "cert-manager"
  repository       = "https://charts.jetstack.io"
  chart            = "cert-manager"
  namespace        = "cert-manager"
  create_namespace = true
  version          = var.cert_manager_version

  set {
    name  = "installCRDs"
    value = "true"
  }

  wait    = true
  timeout = 600

  depends_on = [helm_release.nginx_ingress]
}

# ClusterIssuer para Let's Encrypt (producción)
resource "kubernetes_manifest" "letsencrypt_prod" {
  count = var.enable_ssl ? 1 : 0

  manifest = {
    apiVersion = "cert-manager.io/v1"
    kind       = "ClusterIssuer"
    metadata = {
      name = "letsencrypt-prod"
    }
    spec = {
      acme = {
        server = "https://acme-v02.api.letsencrypt.org/directory"
        email  = var.letsencrypt_email
        privateKeySecretRef = {
          name = "letsencrypt-prod"
        }
        solvers = [
          {
            http01 = {
              ingress = {
                class = "nginx"
              }
            }
          }
        ]
      }
    }
  }

  depends_on = [helm_release.cert_manager]
}

# ClusterIssuer para Let's Encrypt (staging - para pruebas)
resource "kubernetes_manifest" "letsencrypt_staging" {
  count = var.enable_ssl ? 1 : 0

  manifest = {
    apiVersion = "cert-manager.io/v1"
    kind       = "ClusterIssuer"
    metadata = {
      name = "letsencrypt-staging"
    }
    spec = {
      acme = {
        server = "https://acme-staging-v02.api.letsencrypt.org/directory"
        email  = var.letsencrypt_email
        privateKeySecretRef = {
          name = "letsencrypt-staging"
        }
        solvers = [
          {
            http01 = {
              ingress = {
                class = "nginx"
              }
            }
          }
        ]
      }
    }
  }

  depends_on = [helm_release.cert_manager]
}

# Obtener la IP del Load Balancer
data "kubernetes_service" "nginx_ingress" {
  metadata {
    name      = "ingress-nginx-controller"
    namespace = "ingress-nginx"
  }

  depends_on = [helm_release.nginx_ingress]
}
