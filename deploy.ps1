# Script para desplegar CryptoJackpot en DigitalOcean Kubernetes (Windows)

param(
    [string]$Version = "v1.0.0"
)

$ErrorActionPreference = "Stop"

Write-Host "🚀 Iniciando despliegue de CryptoJackpot..." -ForegroundColor Cyan

# Variables
$Registry = "registry.digitalocean.com/cryptojackpot"

Write-Host "📦 Construyendo imágenes Docker con tag: $Version..." -ForegroundColor Yellow

# Build de cada microservicio
docker build -t "$Registry/identity-api:$Version" -f Microservices/Identity/Api/Dockerfile .
docker build -t "$Registry/lottery-api:$Version" -f Microservices/Lottery/Api/Dockerfile .
docker build -t "$Registry/order-api:$Version" -f Microservices/Order/Api/Dockerfile .
docker build -t "$Registry/wallet-api:$Version" -f Microservices/Wallet/Api/Dockerfile .
docker build -t "$Registry/winner-api:$Version" -f Microservices/Winner/Api/Dockerfile .
docker build -t "$Registry/notification-api:$Version" -f Microservices/Notification/Api/Dockerfile .

Write-Host "📤 Subiendo imágenes a DigitalOcean Container Registry..." -ForegroundColor Yellow

docker push "$Registry/identity-api:$Version"
docker push "$Registry/lottery-api:$Version"
docker push "$Registry/order-api:$Version"
docker push "$Registry/wallet-api:$Version"
docker push "$Registry/winner-api:$Version"
docker push "$Registry/notification-api:$Version"

Write-Host "☸️ Aplicando configuraciones de Kubernetes..." -ForegroundColor Yellow

# Aplicar en orden
kubectl apply -f k8s/base/namespace.yaml
kubectl apply -f k8s/base/configmap.yaml
kubectl apply -f k8s/base/secrets.yaml

# NetworkPolicies (seguridad de red)
kubectl apply -f k8s/network/

# Kafka/Redpanda
kubectl apply -f k8s/kafka/redpanda.yaml

# Esperar a que Redpanda esté listo
Write-Host "⏳ Esperando a que Redpanda esté listo..." -ForegroundColor Yellow
kubectl wait --for=condition=ready pod -l app=redpanda -n cryptojackpot --timeout=120s

# Microservicios
kubectl apply -f k8s/microservices/identity/
kubectl apply -f k8s/microservices/lottery/
kubectl apply -f k8s/microservices/order/
kubectl apply -f k8s/microservices/wallet/
kubectl apply -f k8s/microservices/winner/
kubectl apply -f k8s/microservices/notification/

# Ingress
kubectl apply -f k8s/ingress/

Write-Host "✅ Despliegue completado!" -ForegroundColor Green
Write-Host ""
Write-Host "📊 Estado de los pods:" -ForegroundColor Cyan
kubectl get pods -n cryptojackpot
Write-Host ""
Write-Host "🌐 Servicios:" -ForegroundColor Cyan
kubectl get svc -n cryptojackpot

