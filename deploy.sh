#!/bin/bash
# Script para desplegar CryptoJackpot en DigitalOcean Kubernetes

set -e

echo "🚀 Iniciando despliegue de CryptoJackpot..."

# Variables
REGISTRY="registry.digitalocean.com/cryptojackpot"
VERSION=${1:-"v1.0.0"}

echo "📦 Construyendo imágenes Docker con tag: $VERSION..."

# Build de cada microservicio
docker build -t $REGISTRY/identity-api:$VERSION -f Microservices/Identity/Api/Dockerfile .
docker build -t $REGISTRY/lottery-api:$VERSION -f Microservices/Lottery/Api/Dockerfile .
docker build -t $REGISTRY/order-api:$VERSION -f Microservices/Order/Api/Dockerfile .
docker build -t $REGISTRY/wallet-api:$VERSION -f Microservices/Wallet/Api/Dockerfile .
docker build -t $REGISTRY/winner-api:$VERSION -f Microservices/Winner/Api/Dockerfile .
docker build -t $REGISTRY/notification-api:$VERSION -f Microservices/Notification/Api/Dockerfile .

echo "📤 Subiendo imágenes a DigitalOcean Container Registry..."

docker push $REGISTRY/identity-api:$VERSION
docker push $REGISTRY/lottery-api:$VERSION
docker push $REGISTRY/order-api:$VERSION
docker push $REGISTRY/wallet-api:$VERSION
docker push $REGISTRY/winner-api:$VERSION
docker push $REGISTRY/notification-api:$VERSION

echo "☸️ Aplicando configuraciones de Kubernetes..."

# Aplicar en orden
kubectl apply -f infrastructure/k8s/base/namespace.yaml
kubectl apply -f infrastructure/k8s/base/configmap.yaml

# -----------------------------------------------------------------------------
# Secrets - Lógica inteligente para detectar gestión de Terraform
# -----------------------------------------------------------------------------
CONFIG_PATH="deploy-config.json"
if [ -f "$CONFIG_PATH" ]; then
    # Terraform gestiona la infraestructura - los secrets ya están en el cluster
    echo "🔐 Detectada configuración de Terraform..."
    echo "   Los secrets ya fueron aplicados por Terraform al cluster"
    
    # Aplicar archivo generado como actualización si existe
    if [ -f "infrastructure/k8s/base/secrets.generated.yaml" ]; then
        echo "   Aplicando secrets.generated.yaml como actualización..."
        kubectl apply -f infrastructure/k8s/base/secrets.generated.yaml
    fi
elif [ -f "infrastructure/k8s/base/secrets.generated.yaml" ]; then
    # Usar archivo generado por Terraform
    echo "🔐 Usando secrets.generated.yaml (generado por Terraform)..."
    kubectl apply -f infrastructure/k8s/base/secrets.generated.yaml
elif [ -f "infrastructure/k8s/base/secrets.yaml" ]; then
    # Fallback a archivo manual - advertir al usuario
    echo "⚠️ ADVERTENCIA: Usando secrets.yaml (puede contener placeholders)"
    echo "   Asegúrate de haber editado infrastructure/k8s/base/secrets.yaml con valores reales!"
    echo "   Para gestión automatizada, ejecuta: cd infrastructure/terraform && terraform apply"
    read -p "   ¿Continuar? (s/N) " confirm
    if [ "$confirm" != "s" ] && [ "$confirm" != "S" ]; then
        echo "   Cancelado. Edita secrets.yaml o ejecuta Terraform primero."
        exit 1
    fi
    kubectl apply -f infrastructure/k8s/base/secrets.yaml
else
    echo "❌ ERROR: No se encontró ningún archivo de secrets"
    echo "   Ejecuta 'terraform apply' o crea infrastructure/k8s/base/secrets.yaml manualmente"
    exit 1
fi

# NetworkPolicies (seguridad de red)
kubectl apply -f infrastructure/k8s/network/

# Kafka/Redpanda
kubectl apply -f infrastructure/k8s/kafka/redpanda.yaml

# Esperar a que Redpanda esté listo
echo "⏳ Esperando a que Redpanda esté listo..."
kubectl wait --for=condition=ready pod -l app=redpanda -n cryptojackpot --timeout=120s

# Microservicios
kubectl apply -f infrastructure/k8s/microservices/identity/
kubectl apply -f infrastructure/k8s/microservices/lottery/
kubectl apply -f infrastructure/k8s/microservices/order/
kubectl apply -f infrastructure/k8s/microservices/wallet/
kubectl apply -f infrastructure/k8s/microservices/winner/
kubectl apply -f infrastructure/k8s/microservices/notification/

# Ingress namespace y configuración
kubectl apply -f infrastructure/k8s/ingress/namespace.yaml
kubectl label namespace ingress-nginx name=ingress-nginx --overwrite 2>/dev/null || true
kubectl apply -f infrastructure/k8s/ingress/ingress.yaml

echo "✅ Despliegue completado!"
echo ""
echo "📊 Estado de los pods:"
kubectl get pods -n cryptojackpot
echo ""
echo "🌐 Servicios:"
kubectl get svc -n cryptojackpot

