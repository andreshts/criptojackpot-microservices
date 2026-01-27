# Script de desarrollo local con Skaffold
# Inicia todo el stack de CryptoJackpotDistributed en Kubernetes local

param(
    [switch]$Setup,      # Ejecutar setup inicial
    [switch]$Run,        # Modo run (sin watch)
    [switch]$Build,      # Solo construir imágenes
    [switch]$Delete,     # Eliminar todo
    [switch]$Logs,       # Ver logs de todos los servicios
    [switch]$Status,     # Ver estado del cluster
    [string]$Service     # Nombre del servicio para logs específicos
)

$ErrorActionPreference = "Stop"
$rootPath = Split-Path -Parent $PSScriptRoot

function Show-Help {
    Write-Host "CryptoJackpot - Desarrollo Local con Kubernetes" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Uso: .\dev-k8s.ps1 [opciones]" -ForegroundColor White
    Write-Host ""
    Write-Host "Opciones:" -ForegroundColor Yellow
    Write-Host "  -Setup      Configuración inicial (instala ingress, despliega infra)"
    Write-Host "  -Run        Desplegar sin modo watch"
    Write-Host "  -Build      Solo construir imágenes Docker"
    Write-Host "  -Delete     Eliminar todo el despliegue"
    Write-Host "  -Status     Ver estado de todos los pods"
    Write-Host "  -Logs       Ver logs de todos los servicios"
    Write-Host "  -Service X  Ver logs de un servicio específico (ej: identity-api)"
    Write-Host ""
    Write-Host "Sin opciones: Inicia skaffold dev (desarrollo con hot-reload)"
    Write-Host ""
}

function Show-Status {
    Write-Host "Estado del Cluster - CryptoJackpot" -ForegroundColor Cyan
    Write-Host "==================================" -ForegroundColor Cyan
    Write-Host ""
    
    Write-Host "Pods:" -ForegroundColor Yellow
    kubectl get pods -n cryptojackpot -o wide
    Write-Host ""
    
    Write-Host "Services:" -ForegroundColor Yellow
    kubectl get svc -n cryptojackpot
    Write-Host ""
    
    Write-Host "Ingress:" -ForegroundColor Yellow
    kubectl get ingress -n cryptojackpot
    Write-Host ""
}

function Show-Logs {
    param([string]$serviceName)
    
    if ($serviceName) {
        Write-Host "Logs de $serviceName" -ForegroundColor Cyan
        kubectl logs -f deployment/$serviceName -n cryptojackpot
    } else {
        Write-Host "Logs de todos los servicios (Ctrl+C para salir)" -ForegroundColor Cyan
        # Usar stern si está disponible, sino kubectl
        $sternAvailable = Get-Command stern -ErrorAction SilentlyContinue
        if ($sternAvailable) {
            stern -n cryptojackpot ".*-api"
        } else {
            Write-Host "Tip: Instala 'stern' para mejor visualización de logs" -ForegroundColor Yellow
            Write-Host "     winget install -e --id stern.stern" -ForegroundColor Yellow
            Write-Host ""
            # Abrir múltiples ventanas de logs
            $services = @("identity-api", "lottery-api", "order-api", "wallet-api", "winner-api", "notification-api")
            foreach ($svc in $services) {
                Start-Process powershell -ArgumentList "-NoExit", "-Command", "kubectl logs -f deployment/$svc -n cryptojackpot"
            }
        }
    }
}

# Ejecutar acción
Push-Location $rootPath

try {
    if ($Setup) {
        Write-Host "Ejecutando setup inicial..." -ForegroundColor Yellow
        & "$PSScriptRoot\setup-local-k8s.ps1"
    }
    elseif ($Build) {
        Write-Host "Construyendo imágenes..." -ForegroundColor Yellow
        skaffold build
    }
    elseif ($Run) {
        Write-Host "Desplegando (modo run)..." -ForegroundColor Yellow
        skaffold run
    }
    elseif ($Delete) {
        Write-Host "Eliminando despliegue..." -ForegroundColor Yellow
        skaffold delete
        Write-Host "Despliegue eliminado." -ForegroundColor Green
    }
    elseif ($Status) {
        Show-Status
    }
    elseif ($Logs) {
        Show-Logs -serviceName $Service
    }
    elseif ($Service) {
        Show-Logs -serviceName $Service
    }
    else {
        # Modo por defecto: desarrollo con hot-reload
        Write-Host "========================================" -ForegroundColor Cyan
        Write-Host " CryptoJackpot - Modo Desarrollo" -ForegroundColor Cyan
        Write-Host "========================================" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Iniciando Skaffold en modo dev..." -ForegroundColor Yellow
        Write-Host "Los servicios se reconstruirán automáticamente al detectar cambios." -ForegroundColor Gray
        Write-Host "Presiona Ctrl+C para detener." -ForegroundColor Gray
        Write-Host ""
        
        skaffold dev -p dev
    }
}
finally {
    Pop-Location
}
