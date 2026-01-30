# Script de Setup para Kubernetes Local
# CryptoJackpotDistributed

param(
    [switch]$SkipIngress,
    [switch]$UseMinikube,
    [switch]$Verbose
)

$ErrorActionPreference = "Continue"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " CryptoJackpot - Setup Kubernetes Local" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar prerrequisitos
function Test-Prerequisites {
    Write-Host "[1/6] Verificando prerrequisitos..." -ForegroundColor Yellow
    
    # Docker
    try {
        $dockerVersion = docker version --format '{{.Server.Version}}' 2>$null
        Write-Host "  ✓ Docker: $dockerVersion" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ Docker no encontrado. Instala Docker Desktop." -ForegroundColor Red
        exit 1
    }
    
    # Kubectl
    try {
        $kubectlOutput = kubectl version --client -o json 2>$null | ConvertFrom-Json
        $kubectlVersion = $kubectlOutput.clientVersion.gitVersion
        Write-Host "  ✓ kubectl: $kubectlVersion" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ kubectl no encontrado." -ForegroundColor Red
        Write-Host "    Instalar con: winget install -e --id Kubernetes.kubectl" -ForegroundColor Yellow
        exit 1
    }
    
    # Skaffold
    try {
        $skaffoldVersion = skaffold version 2>$null
        Write-Host "  ✓ Skaffold: $skaffoldVersion" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ Skaffold no encontrado." -ForegroundColor Red
        Write-Host "    Instalar con: winget install -e --id Google.ContainerTools.Skaffold" -ForegroundColor Yellow
        exit 1
    }
    
    # Verificar cluster de Kubernetes
    try {
        $context = kubectl config current-context 2>$null
        Write-Host "  ✓ Kubernetes context: $context" -ForegroundColor Green
        
        $nodes = kubectl get nodes --no-headers 2>$null
        if ($nodes) {
            Write-Host "  ✓ Cluster activo" -ForegroundColor Green
        } else {
            throw "No nodes"
        }
    } catch {
        Write-Host "  ✗ Cluster de Kubernetes no disponible." -ForegroundColor Red
        Write-Host "    Opciones:" -ForegroundColor Yellow
        Write-Host "    - Docker Desktop: Habilitar Kubernetes en Settings" -ForegroundColor Yellow
        Write-Host "    - Minikube: minikube start" -ForegroundColor Yellow
        Write-Host "    - Kind: kind create cluster" -ForegroundColor Yellow
        exit 1
    }
    
    Write-Host ""
}

# Configurar contexto de Kubernetes
function Set-KubernetesContext {
    Write-Host "[2/6] Configurando contexto de Kubernetes..." -ForegroundColor Yellow
    
    if ($UseMinikube) {
        Write-Host "  Usando Minikube..." -ForegroundColor Cyan
        minikube status | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  Iniciando Minikube..." -ForegroundColor Cyan
            minikube start --cpus=4 --memory=8192
        }
        kubectl config use-context minikube
        
        # Habilitar addons necesarios
        Write-Host "  Habilitando addons de Minikube..." -ForegroundColor Cyan
        minikube addons enable ingress
        minikube addons enable storage-provisioner
    } else {
        Write-Host "  Usando contexto actual: $(kubectl config current-context)" -ForegroundColor Cyan
    }
    
    Write-Host ""
}

# Instalar NGINX Ingress Controller
function Install-IngressController {
    if ($SkipIngress) {
        Write-Host "[3/6] Saltando instalación de Ingress Controller..." -ForegroundColor Yellow
        return
    }
    
    Write-Host "[3/6] Instalando NGINX Ingress Controller..." -ForegroundColor Yellow
    
    # Verificar si ya está instalado
    $ingressPods = $null
    try {
        $ingressPods = kubectl get pods -n ingress-nginx --no-headers 2>&1 | Where-Object { $_ -notmatch "No resources found" }
    } catch {}
    
    if ($ingressPods -and $ingressPods.Length -gt 0) {
        Write-Host "  ✓ NGINX Ingress Controller ya está instalado" -ForegroundColor Green
    } else {
        Write-Host "  Instalando NGINX Ingress Controller..." -ForegroundColor Cyan
        
        # Usar el manifest oficial de NGINX
        kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.9.5/deploy/static/provider/cloud/deploy.yaml
        
        Write-Host "  Esperando a que Ingress Controller esté listo..." -ForegroundColor Cyan
        kubectl wait --namespace ingress-nginx `
            --for=condition=ready pod `
            --selector=app.kubernetes.io/component=controller `
            --timeout=120s
        
        Write-Host "  ✓ NGINX Ingress Controller instalado" -ForegroundColor Green
    }
    
    Write-Host ""
}

# Crear namespace y recursos base
function Deploy-BaseResources {
    Write-Host "[4/6] Desplegando recursos base..." -ForegroundColor Yellow
    
    $basePath = "$PSScriptRoot"
    
    # Namespace
    kubectl apply -f "$basePath\namespace.yaml"
    Write-Host "  ✓ Namespace creado" -ForegroundColor Green
    
    # ConfigMap
    kubectl apply -f "$basePath\configmap.yaml"
    Write-Host "  ✓ ConfigMap creado" -ForegroundColor Green
    
    # Secrets
    kubectl apply -f "$basePath\secrets.yaml"
    Write-Host "  ✓ Secrets creados" -ForegroundColor Green
    
    Write-Host ""
}

# Desplegar infraestructura
function Deploy-Infrastructure {
    Write-Host "[5/6] Desplegando infraestructura..." -ForegroundColor Yellow
    
    $basePath = "$PSScriptRoot"
    
    # PostgreSQL
    Write-Host "  Desplegando PostgreSQL..." -ForegroundColor Cyan
    kubectl apply -f "$basePath\postgres\postgres.yaml"
    
    # Esperar a que PostgreSQL esté listo
    Write-Host "  Esperando a que PostgreSQL esté listo..." -ForegroundColor Cyan
    $maxRetries = 60
    $retryCount = 0
    while ($retryCount -lt $maxRetries) {
        $ready = kubectl get pod postgres-0 -n cryptojackpot -o jsonpath='{.status.conditions[?(@.type=="Ready")].status}' 2>$null
        if ($ready -eq "True") {
            break
        }
        Start-Sleep -Seconds 2
        $retryCount++
        Write-Host "    Esperando... ($retryCount/$maxRetries)" -ForegroundColor Gray
    }
    Write-Host "  ✓ PostgreSQL listo" -ForegroundColor Green
    
    # PgBouncer (Connection Pooler)
    Write-Host "  Desplegando PgBouncer (Connection Pooler)..." -ForegroundColor Cyan
    kubectl apply -f "$basePath\postgres\pgbouncer.yaml"
    
    # Esperar a que PgBouncer esté listo
    Write-Host "  Esperando a que PgBouncer esté listo..." -ForegroundColor Cyan
    $retryCount = 0
    while ($retryCount -lt $maxRetries) {
        $ready = kubectl get pod -l app=pgbouncer -n cryptojackpot -o jsonpath='{.items[0].status.conditions[?(@.type=="Ready")].status}' 2>$null
        if ($ready -eq "True") {
            break
        }
        Start-Sleep -Seconds 2
        $retryCount++
        Write-Host "    Esperando... ($retryCount/$maxRetries)" -ForegroundColor Gray
    }
    Write-Host "  ✓ PgBouncer listo" -ForegroundColor Green
    
    # Redpanda
    Write-Host "  Desplegando Redpanda (Kafka)..." -ForegroundColor Cyan
    kubectl apply -f "$basePath\redpanda\redpanda.yaml"
    
    # Esperar a que Redpanda esté listo
    Write-Host "  Esperando a que Redpanda esté listo..." -ForegroundColor Cyan
    $retryCount = 0
    while ($retryCount -lt $maxRetries) {
        $ready = kubectl get pod redpanda-0 -n cryptojackpot -o jsonpath='{.status.conditions[?(@.type=="Ready")].status}' 2>$null
        if ($ready -eq "True") {
            break
        }
        Start-Sleep -Seconds 2
        $retryCount++
        Write-Host "    Esperando... ($retryCount/$maxRetries)" -ForegroundColor Gray
    }
    Write-Host "  ✓ Redpanda listo" -ForegroundColor Green
    
    # MinIO
    Write-Host "  Desplegando MinIO (S3)..." -ForegroundColor Cyan
    kubectl apply -f "$basePath\minio\minio.yaml"
    Write-Host "  ✓ MinIO desplegado" -ForegroundColor Green
    
    # MongoDB
    Write-Host "  Desplegando MongoDB..." -ForegroundColor Cyan
    kubectl apply -f "$basePath\secrets\mongodb-secrets.yaml"
    kubectl apply -f "$basePath\mongodb\deployment.yaml"
    
    # Esperar a que MongoDB esté listo
    Write-Host "  Esperando a que MongoDB esté listo..." -ForegroundColor Cyan
    $retryCount = 0
    while ($retryCount -lt $maxRetries) {
        $ready = kubectl get pod -l app=mongodb -n cryptojackpot -o jsonpath='{.items[0].status.conditions[?(@.type=="Ready")].status}' 2>$null
        if ($ready -eq "True") {
            break
        }
        Start-Sleep -Seconds 2
        $retryCount++
        Write-Host "    Esperando... ($retryCount/$maxRetries)" -ForegroundColor Gray
    }
    Write-Host "  ✓ MongoDB listo" -ForegroundColor Green
    
    # Redis (SignalR Backplane)
    Write-Host "  Desplegando Redis (SignalR Backplane)..." -ForegroundColor Cyan
    kubectl apply -f "$basePath\redis\redis-deployment.yaml"
    kubectl apply -f "$basePath\redis\redis-service.yaml"
    
    # Esperar a que Redis esté listo
    Write-Host "  Esperando a que Redis esté listo..." -ForegroundColor Cyan
    $retryCount = 0
    while ($retryCount -lt $maxRetries) {
        $ready = kubectl get pod -l app=redis -n cryptojackpot -o jsonpath='{.items[0].status.conditions[?(@.type=="Ready")].status}' 2>$null
        if ($ready -eq "True") {
            break
        }
        Start-Sleep -Seconds 2
        $retryCount++
        Write-Host "    Esperando... ($retryCount/$maxRetries)" -ForegroundColor Gray
    }
    Write-Host "  ✓ Redis listo" -ForegroundColor Green
    
    Write-Host ""
}

# Información final
function Show-Summary {
    Write-Host "[6/6] Configuración completada!" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host " Infraestructura lista para desarrollo" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Para desplegar los microservicios, ejecuta:" -ForegroundColor White
    Write-Host "  cd $PSScriptRoot\..\.." -ForegroundColor Yellow
    Write-Host "  skaffold dev -p dev" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "O para desplegar sin modo watch:" -ForegroundColor White
    Write-Host "  skaffold run" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Servicios disponibles después del despliegue:" -ForegroundColor White
    Write-Host "  - Identity API:     http://localhost:5001" -ForegroundColor Gray
    Write-Host "  - Lottery API:      http://localhost:5002" -ForegroundColor Gray
    Write-Host "  - Order API:        http://localhost:5003" -ForegroundColor Gray
    Write-Host "  - Wallet API:       http://localhost:5004" -ForegroundColor Gray
    Write-Host "  - Winner API:       http://localhost:5005" -ForegroundColor Gray
    Write-Host "  - Notification API: http://localhost:5006" -ForegroundColor Gray
    Write-Host "  - Audit API:        http://localhost:5007" -ForegroundColor Gray
    Write-Host "  - PostgreSQL:       localhost:5433" -ForegroundColor Gray
    Write-Host "  - MongoDB:          localhost:27017" -ForegroundColor Gray
    Write-Host "  - Redis:            localhost:6379" -ForegroundColor Gray
    Write-Host "  - Kafka (Redpanda): localhost:9092" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Para ver los pods:" -ForegroundColor White
    Write-Host "  kubectl get pods -n cryptojackpot" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Para ver los logs de un servicio:" -ForegroundColor White
    Write-Host "  kubectl logs -f deployment/identity-api -n cryptojackpot" -ForegroundColor Yellow
    Write-Host ""
}

# Ejecutar
Test-Prerequisites
Set-KubernetesContext
Install-IngressController
Deploy-BaseResources
Deploy-Infrastructure
Show-Summary
