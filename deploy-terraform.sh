#!/bin/bash
# =============================================================================
# Script de Despliegue CryptoJackpot - Linux/macOS
# Integrado con Terraform IaC
# =============================================================================

set -e

# Parámetros
VERSION="${1:-v1.0.0}"
SKIP_BUILD="${2:-false}"
SKIP_SECRETS="${3:-false}"

echo "🚀 Iniciando despliegue de CryptoJackpot..."

# -----------------------------------------------------------------------------
# Configuración desde Terraform (si existe)
# -----------------------------------------------------------------------------
CONFIG_PATH="deploy-config.json"
REGISTRY="registry.digitalocean.com/cryptojackpot"
TERRAFORM_MANAGED=false

if [ -f "$CONFIG_PATH" ]; then
    echo "📄 Usando configuración de Terraform..."
    REGISTRY=$(jq -r '.registry_url' $CONFIG_PATH)
    CLUSTER_NAME=$(jq -r '.cluster_name' $CONFIG_PATH)
    ENVIRONMENT=$(jq -r '.environment' $CONFIG_PATH)
    TERRAFORM_MANAGED=true
    echo "   Registry: $REGISTRY"
    echo "   Cluster: $CLUSTER_NAME"
    echo "   Environment: $ENVIRONMENT"
else
    echo "⚠️ deploy-config.json no encontrado, usando valores por defecto"
    echo "   Para configuración automatizada, ejecuta primero:"
    echo "   cd infrastructure/terraform && terraform apply"
fi

# -----------------------------------------------------------------------------
# Build de Imágenes Docker
# -----------------------------------------------------------------------------
if [ "$SKIP_BUILD" != "true" ]; then
    echo "📦 Construyendo imágenes Docker con tag: $VERSION..."

    docker build -t "$REGISTRY/identity-api:$VERSION" -f Microservices/Identity/Api/Dockerfile .
    docker build -t "$REGISTRY/lottery-api:$VERSION" -f Microservices/Lottery/Api/Dockerfile .
    docker build -t "$REGISTRY/order-api:$VERSION" -f Microservices/Order/Api/Dockerfile .
    docker build -t "$REGISTRY/wallet-api:$VERSION" -f Microservices/Wallet/Api/Dockerfile .
    docker build -t "$REGISTRY/winner-api:$VERSION" -f Microservices/Winner/Api/Dockerfile .
    docker build -t "$REGISTRY/notification-api:$VERSION" -f Microservices/Notification/Api/Dockerfile .

    echo "📤 Subiendo imágenes a DigitalOcean Container Registry..."

    docker push "$REGISTRY/identity-api:$VERSION"
    docker push "$REGISTRY/lottery-api:$VERSION"
    docker push "$REGISTRY/order-api:$VERSION"
    docker push "$REGISTRY/wallet-api:$VERSION"
    docker push "$REGISTRY/winner-api:$VERSION"
    docker push "$REGISTRY/notification-api:$VERSION"
else
    echo "⏭️ Saltando build de imágenes"
fi

# -----------------------------------------------------------------------------
# Actualizar tags de imágenes en deployments
# -----------------------------------------------------------------------------
echo "🔄 Actualizando tags de imágenes en manifests..."

for svc in identity lottery order wallet winner notification; do
    DEPLOYMENT_PATH="k8s/microservices/$svc/deployment.yaml"
    if [ -f "$DEPLOYMENT_PATH" ]; then
        sed -i.bak "s|registry.digitalocean.com/cryptojackpot/$svc-api:v[0-9.]*|$REGISTRY/$svc-api:$VERSION|g" "$DEPLOYMENT_PATH"
        rm -f "$DEPLOYMENT_PATH.bak"
    fi
done

# -----------------------------------------------------------------------------
# Aplicar Kubernetes Manifests
# -----------------------------------------------------------------------------
echo "☸️ Aplicando configuraciones de Kubernetes..."

# Namespace y ConfigMap
kubectl apply -f k8s/base/namespace.yaml
kubectl apply -f k8s/base/configmap.yaml

# -----------------------------------------------------------------------------
# Secrets - Lógica mejorada para Terraform
# -----------------------------------------------------------------------------
if [ "$SKIP_SECRETS" = "true" ]; then
    echo "⏭️ Saltando aplicación de secrets (SKIP_SECRETS=true)"
    echo "   Asumiendo que Terraform ya los aplicó al cluster"
elif [ "$TERRAFORM_MANAGED" = "true" ]; then
    # Si Terraform gestiona la infra, los secrets ya están en el cluster
    echo "🔐 Infraestructura gestionada por Terraform..."
    echo "   Los secrets (postgres, jwt, spaces, kafka) ya están en el cluster"
    
    # Solo aplicar el archivo generado como backup/actualización si existe
    if [ -f "k8s/base/secrets.generated.yaml" ]; then
        echo "   Aplicando secrets.generated.yaml como actualización..."
        kubectl apply -f k8s/base/secrets.generated.yaml
    fi
else
    # Sin Terraform - usar archivo manual
    echo "⚠️ Sin Terraform - usando secrets manuales..."
    
    if [ -f "k8s/base/secrets.generated.yaml" ]; then
        echo "   Encontrado secrets.generated.yaml - usando este archivo"
        kubectl apply -f k8s/base/secrets.generated.yaml
    elif [ -f "k8s/base/secrets.yaml" ]; then
        echo "   ⚠️ ADVERTENCIA: Usando secrets.yaml con placeholders"
        echo "   Asegúrate de haber editado k8s/base/secrets.yaml con valores reales!"
        read -p "   ¿Continuar? (s/N) " confirm
        if [ "$confirm" != "s" ] && [ "$confirm" != "S" ]; then
            echo "   Cancelado. Edita secrets.yaml o ejecuta Terraform primero."
            exit 1
        fi
        kubectl apply -f k8s/base/secrets.yaml
    else
        echo "❌ ERROR: No se encontró ningún archivo de secrets"
        echo "   Ejecuta 'terraform apply' o crea k8s/base/secrets.yaml manualmente"
        exit 1
    fi
fi

# -----------------------------------------------------------------------------
# NetworkPolicies (seguridad de red)
# -----------------------------------------------------------------------------
kubectl apply -f k8s/network/

# -----------------------------------------------------------------------------
# Kafka/Redpanda
# NOTA: El secret redpanda-credentials es gestionado por Terraform
# El archivo redpanda.yaml solo contiene ConfigMap y StatefulSet
# -----------------------------------------------------------------------------
echo "🔄 Desplegando Redpanda (Kafka)..."
kubectl apply -f k8s/kafka/redpanda.yaml

# Esperar a que Redpanda esté listo
echo "⏳ Esperando a que Redpanda esté listo..."
kubectl wait --for=condition=ready pod -l app=redpanda -n cryptojackpot --timeout=180s

# -----------------------------------------------------------------------------
# Microservicios
# -----------------------------------------------------------------------------
echo "🚀 Desplegando microservicios..."
kubectl apply -f k8s/microservices/identity/
kubectl apply -f k8s/microservices/lottery/
kubectl apply -f k8s/microservices/order/
kubectl apply -f k8s/microservices/wallet/
kubectl apply -f k8s/microservices/winner/
kubectl apply -f k8s/microservices/notification/

# -----------------------------------------------------------------------------
# Ingress
# -----------------------------------------------------------------------------
kubectl apply -f k8s/ingress/namespace.yaml
kubectl label namespace ingress-nginx name=ingress-nginx --overwrite 2>/dev/null || true
kubectl apply -f k8s/ingress/ingress.yaml

# -----------------------------------------------------------------------------
# Resumen del Despliegue
# -----------------------------------------------------------------------------
echo ""
echo "============================================"
echo "✅ Despliegue completado!"
echo "============================================"
echo ""
echo "📊 Estado de los pods:"
kubectl get pods -n cryptojackpot
echo ""
echo "🌐 Servicios:"
kubectl get svc -n cryptojackpot
echo ""
echo "🔗 Ingress:"
kubectl get ingress -n cryptojackpot
echo ""

# Mostrar IP del Load Balancer
LB_IP=$(kubectl get svc -n ingress-nginx ingress-nginx-controller -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || echo "")
if [ -n "$LB_IP" ]; then
    echo "🌍 Load Balancer IP: $LB_IP"
    echo "   Configura tu DNS para apuntar a esta IP"
fi

echo ""
echo "📝 Comandos útiles:"
echo "   kubectl logs -f deployment/identity-api -n cryptojackpot"
echo "   kubectl get events -n cryptojackpot --sort-by='.lastTimestamp'"

