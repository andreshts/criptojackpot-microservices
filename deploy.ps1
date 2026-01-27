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
kubectl apply -f infrastructure/k8s/base/namespace.yaml
kubectl apply -f infrastructure/k8s/base/configmap.yaml

# -----------------------------------------------------------------------------
# Secrets - Lógica inteligente para detectar gestión de Terraform
# -----------------------------------------------------------------------------
$configPath = "deploy-config.json"
if (Test-Path $configPath) {
    # Terraform gestiona la infraestructura - los secrets ya están en el cluster
    Write-Host "🔐 Detectada configuración de Terraform..." -ForegroundColor Green
    Write-Host "   Los secrets ya fueron aplicados por Terraform al cluster" -ForegroundColor Gray
    
    # Aplicar archivo generado como actualización si existe
    if (Test-Path "infrastructure/k8s/base/secrets.generated.yaml") {
        Write-Host "   Aplicando secrets.generated.yaml como actualización..." -ForegroundColor Gray
        kubectl apply -f infrastructure/k8s/base/secrets.generated.yaml
    }
}
elseif (Test-Path "infrastructure/k8s/base/secrets.generated.yaml") {
    # Usar archivo generado por Terraform
    Write-Host "🔐 Usando secrets.generated.yaml (generado por Terraform)..." -ForegroundColor Green
    kubectl apply -f infrastructure/k8s/base/secrets.generated.yaml
}
elseif (Test-Path "infrastructure/k8s/base/secrets.yaml") {
    # Fallback a archivo manual - advertir al usuario
    Write-Host "⚠️ ADVERTENCIA: Usando secrets.yaml (puede contener placeholders)" -ForegroundColor Red
    Write-Host "   Asegúrate de haber editado infrastructure/k8s/base/secrets.yaml con valores reales!" -ForegroundColor Red
    Write-Host "   Para gestión automatizada, ejecuta: cd infrastructure/terraform && terraform apply" -ForegroundColor Yellow
    $confirm = Read-Host "   ¿Continuar? (s/N)"
    if ($confirm -ne "s" -and $confirm -ne "S") {
        Write-Host "   Cancelado. Edita secrets.yaml o ejecuta Terraform primero." -ForegroundColor Yellow
        exit 1
    }
    kubectl apply -f infrastructure/k8s/base/secrets.yaml
}
else {
    Write-Host "❌ ERROR: No se encontró ningún archivo de secrets" -ForegroundColor Red
    Write-Host "   Ejecuta 'terraform apply' o crea infrastructure/k8s/base/secrets.yaml manualmente" -ForegroundColor Red
    exit 1
}

# NetworkPolicies (seguridad de red)
kubectl apply -f infrastructure/k8s/network/

# Kafka/Redpanda
kubectl apply -f infrastructure/k8s/kafka/redpanda.yaml

# Esperar a que Redpanda esté listo
Write-Host "⏳ Esperando a que Redpanda esté listo..." -ForegroundColor Yellow
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
kubectl label namespace ingress-nginx name=ingress-nginx --overwrite 2>$null
kubectl apply -f infrastructure/k8s/ingress/ingress.yaml

Write-Host "✅ Despliegue completado!" -ForegroundColor Green
Write-Host ""
Write-Host "📊 Estado de los pods:" -ForegroundColor Cyan
kubectl get pods -n cryptojackpot
Write-Host ""
Write-Host "🌐 Servicios:" -ForegroundColor Cyan
kubectl get svc -n cryptojackpot

