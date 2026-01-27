# Desarrollo Local con Kubernetes y Skaffold

Este directorio contiene la configuración para replicar el entorno de producción localmente usando Kubernetes.

## 📋 Requisitos Previos

### Software Necesario

1. **Docker Desktop** con Kubernetes habilitado
   - O alternativamente: **Minikube** / **Kind** / **k3d**
   
2. **kubectl** - CLI de Kubernetes
   ```powershell
   winget install -e --id Kubernetes.kubectl
   ```

3. **Skaffold** - Automatización de desarrollo
   
   **Opción A: Con winget (recomendada)**
   ```powershell
   winget install -e --id Google.ContainerTools.Skaffold
   ```
   
   **Opción B: Con Chocolatey**
   ```powershell
   choco install skaffold
   ```
   
   **Opción C: Descarga directa**
   ```powershell
   # Descargar el ejecutable
   Invoke-WebRequest -Uri "https://storage.googleapis.com/skaffold/releases/latest/skaffold-windows-amd64.exe" -OutFile "$env:USERPROFILE\skaffold.exe"
   
   # Mover a un directorio en PATH (ejecutar como Admin)
   Move-Item "$env:USERPROFILE\skaffold.exe" "C:\Windows\System32\skaffold.exe"
   ```
   
   ```powershell
   # Verificar instalación
   skaffold version
   ```

4. **Helm** (opcional, para NGINX Ingress Controller)
   ```powershell
   winget install -e --id Helm.Helm
   ```

### Recursos Recomendados

- **CPU**: 4+ cores
- **RAM**: 8GB+ (recomendado 16GB)
- **Disco**: 20GB+ libres

## 🚀 Inicio Rápido

### Opción 1: Docker Desktop Kubernetes

1. Abre Docker Desktop → Settings → Kubernetes → Enable Kubernetes
2. Espera a que Kubernetes inicie (icono verde)
3. Ejecuta el script de setup:

```powershell
.\setup-local-k8s.ps1
```

### Opción 2: Minikube

```powershell
# Iniciar Minikube con recursos adecuados
minikube start --cpus=4 --memory=8192 --driver=docker

# Habilitar ingress
minikube addons enable ingress

# Ejecutar setup
.\setup-local-k8s.ps1
```

### Opción 3: Kind (Kubernetes in Docker)

```powershell
# Crear cluster
kind create cluster --name cryptojackpot --config kind-config.yaml

# Ejecutar setup
.\setup-local-k8s.ps1
```

## 📁 Estructura de Archivos

```
k8s/local/
├── namespace.yaml          # Namespace cryptojackpot
├── configmap.yaml          # Configuraciones de entorno
├── secrets.yaml            # Secretos (desarrollo)
├── postgres/
│   └── postgres.yaml       # PostgreSQL StatefulSet
├── redpanda/
│   └── redpanda.yaml       # Redpanda (Kafka) + Console
├── minio/
│   └── minio.yaml          # MinIO (S3 compatible)
├── microservices/
│   ├── identity/
│   ├── lottery/
│   ├── order/
│   ├── wallet/
│   ├── winner/
│   └── notification/
└── ingress/
    └── ingress.yaml        # NGINX Ingress
```

## 🔧 Comandos de Skaffold

### Desarrollo con Hot-Reload

```powershell
# Modo desarrollo (reconstruye al detectar cambios)
skaffold dev -p dev

# O desde la raíz del proyecto
skaffold dev
```

### Build y Deploy Manual

```powershell
# Solo construir imágenes
skaffold build

# Desplegar sin modo watch
skaffold run

# Eliminar todo
skaffold delete
```

### Debugging

```powershell
# Con debugging habilitado
skaffold debug -p dev
```

## 🌐 Acceso a Servicios

### Port Forwarding (Automático con Skaffold)

Cuando ejecutas `skaffold dev`, automáticamente se configuran:

| Servicio | Puerto Local | Descripción |
|----------|-------------|-------------|
| Identity API | 5001 | Autenticación y usuarios |
| Lottery API | 5002 | Gestión de loterías |
| Order API | 5003 | Órdenes y tickets |
| Wallet API | 5004 | Billeteras |
| Winner API | 5005 | Ganadores |
| Notification API | 5006 | Notificaciones |
| PostgreSQL | 5433 | Base de datos |
| Redpanda (Kafka) | 9092 | Message broker |

### Acceso via Ingress

Agregar a `C:\Windows\System32\drivers\etc\hosts`:
```
127.0.0.1 cryptojackpot.local
127.0.0.1 tools.cryptojackpot.local
```

Luego acceder a:
- **API**: http://cryptojackpot.local/api/v1/...
- **Kafka Console**: http://tools.cryptojackpot.local/kafka
- **MinIO Console**: http://tools.cryptojackpot.local/minio

### Port Forward Manual

```powershell
# PostgreSQL
kubectl port-forward svc/postgres -n cryptojackpot 5433:5432

# Redpanda Console
kubectl port-forward svc/redpanda-console -n cryptojackpot 8080:8080

# MinIO Console
kubectl port-forward svc/minio -n cryptojackpot 9001:9001

# Identity API
kubectl port-forward svc/identity-api -n cryptojackpot 5001:80
```

## 🔍 Comandos Útiles

### Ver Estado del Cluster

```powershell
# Todos los recursos
kubectl get all -n cryptojackpot

# Pods con más detalles
kubectl get pods -n cryptojackpot -o wide

# Servicios
kubectl get svc -n cryptojackpot

# Logs de un pod
kubectl logs -f deployment/identity-api -n cryptojackpot

# Describir un pod (para debugging)
kubectl describe pod <pod-name> -n cryptojackpot
```

### Conectar a PostgreSQL

```powershell
# Conectar al pod de PostgreSQL
kubectl exec -it postgres-0 -n cryptojackpot -- psql -U postgres

# Listar bases de datos
\l

# Conectar a una base específica
\c cryptojackpot_identity_db
```

### Ver Topics de Kafka

```powershell
# Conectar al pod de Redpanda
kubectl exec -it redpanda-0 -n cryptojackpot -- rpk topic list
```

## 🐛 Troubleshooting

### Los pods no inician

```powershell
# Ver eventos del namespace
kubectl get events -n cryptojackpot --sort-by='.lastTimestamp'

# Ver logs del pod
kubectl logs <pod-name> -n cryptojackpot --previous
```

### PostgreSQL no está listo

```powershell
# Verificar PVC
kubectl get pvc -n cryptojackpot

# Ver logs de PostgreSQL
kubectl logs postgres-0 -n cryptojackpot
```

### Imágenes no se encuentran

```powershell
# Reconstruir imágenes
skaffold build

# O forzar rebuild
skaffold dev --force
```

### Recursos insuficientes

```powershell
# Ver uso de recursos
kubectl top pods -n cryptojackpot
kubectl top nodes

# Reducir réplicas si es necesario
kubectl scale deployment --all --replicas=1 -n cryptojackpot
```

## 🔄 Diferencias con Producción

| Aspecto | Local | Producción |
|---------|-------|------------|
| PostgreSQL | StatefulSet local | DigitalOcean Managed |
| Kafka | Redpanda sin SASL | Redpanda con SASL |
| Storage | emptyDir/PVC local | DigitalOcean Volumes |
| S3 | MinIO | DigitalOcean Spaces |
| Ingress | NGINX local | NGINX + Cloudflare |
| TLS | Sin TLS | Let's Encrypt |
| Réplicas | 1 por servicio | 2+ por servicio |
| Resources | Limits reducidos | Limits de producción |

## 🧹 Limpieza

```powershell
# Eliminar todo el despliegue
skaffold delete

# O manualmente
kubectl delete namespace cryptojackpot

# Limpiar imágenes de Docker
docker image prune -a
```

## 📊 Monitoreo Local (Opcional)

Para agregar monitoreo similar a producción:

```powershell
# Instalar Prometheus + Grafana con Helm
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm install prometheus prometheus-community/kube-prometheus-stack -n monitoring --create-namespace
```
