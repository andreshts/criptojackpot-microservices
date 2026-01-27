# Kubernetes Deployment - CryptoJackpotDistributed

## Estructura de Directorios

```
k8s/
├── local/                   # ← Desarrollo local (Docker Desktop / Minikube)
│   ├── namespace.yaml
│   ├── configmap.yaml
│   ├── secrets/             # Secrets para desarrollo
│   ├── postgres/            # PostgreSQL local en cluster
│   ├── redpanda/            # Redpanda (Kafka) local
│   ├── minio/               # MinIO (S3 compatible) local
│   ├── microservices/       # Deployments locales
│   ├── ingress/             # Ingress local
│   └── setup-local-k8s.ps1  # Script de setup automatizado
│
├── prod/                    # ← Producción (DigitalOcean / AWS / GCP)
│   ├── base/                # Configuraciones base
│   │   ├── namespace.yaml
│   │   ├── configmap.yaml
│   │   └── secrets.yaml     # ⚠️ Plantilla - NO commitar valores reales
│   ├── databases/           # Scripts de inicialización
│   │   └── init-databases.sql
│   ├── network/             # NetworkPolicies para seguridad
│   │   └── network-policies.yaml
│   ├── microservices/       # Deployments de producción
│   │   ├── identity/
│   │   ├── lottery/
│   │   ├── order/
│   │   ├── wallet/
│   │   ├── winner/
│   │   └── notification/
│   ├── ingress/             # Ingress con TLS/SSL
│   │   └── ingress.yaml
│   └── kafka/               # Redpanda con SASL para producción
│       └── redpanda.yaml
│
└── README.md                # Este archivo
```

---

## 🏠 Desarrollo Local

### Prerrequisitos
- Docker Desktop con Kubernetes habilitado (o Minikube)
- kubectl
- Skaffold

### Setup Rápido

```powershell
# Windows PowerShell
cd k8s\local
.\setup-local-k8s.ps1

# Luego desplegar con Skaffold
cd ..\..
skaffold dev -p dev
```

### Servicios Locales
| Servicio | URL |
|----------|-----|
| Identity API | http://localhost:5001 |
| Lottery API | http://localhost:5002 |
| Order API | http://localhost:5003 |
| Wallet API | http://localhost:5004 |
| Winner API | http://localhost:5005 |
| Notification API | http://localhost:5006 |
| PostgreSQL | localhost:5433 |
| Kafka (Redpanda) | localhost:9092 |
| Redpanda Console | http://localhost:8080 |
| MinIO Console | http://localhost:9001 |

### Comandos Útiles (Local)

```bash
# Ver pods
kubectl get pods -n cryptojackpot

# Ver logs de un servicio
kubectl logs -f deployment/identity-api -n cryptojackpot

# Reiniciar un deployment
kubectl rollout restart deployment/identity-api -n cryptojackpot

# Port-forward PostgreSQL
kubectl port-forward svc/postgres 5433:5432 -n cryptojackpot

# Limpiar todo
kubectl delete namespace cryptojackpot
```

---

## 🚀 Producción (DigitalOcean)

### Arquitectura

```
                    ┌─────────────────────┐
                    │   Cloudflare CDN    │
                    │   (DNS + SSL/TLS)   │
                    └──────────┬──────────┘
                               │
                    ┌──────────▼──────────┐
                    │   NGINX Ingress     │
                    │   Controller        │
                    └──────────┬──────────┘
                               │
        ┌──────────────────────┼──────────────────────┐
        │                      │                      │
   ┌────▼────┐           ┌────▼────┐           ┌────▼────┐
   │Identity │           │ Lottery │           │  Order  │
   │   API   │           │   API   │           │   API   │
   └────┬────┘           └────┬────┘           └────┬────┘
        │                      │                      │
        └──────────────────────┼──────────────────────┘
                               │
                    ┌──────────▼──────────┐
                    │   Redpanda (Kafka)  │
                    │   DigitalOcean      │
                    └──────────┬──────────┘
                               │
                    ┌──────────▼──────────┐
                    │   PostgreSQL        │
                    │   Managed DB        │
                    └─────────────────────┘
```

### Base de Datos

Usamos **PostgreSQL Managed** en DigitalOcean con **6 databases separadas**:

```
PostgreSQL Server (DigitalOcean Managed)
├── cryptojackpot_identity_db
├── cryptojackpot_lottery_db
├── cryptojackpot_order_db
├── cryptojackpot_wallet_db
├── cryptojackpot_winner_db
└── cryptojackpot_notification_db
```

### Despliegue de Producción

```bash
# 1. Configurar secrets (NUNCA commitear valores reales)
cp k8s/prod/base/secrets.yaml k8s/prod/base/secrets.local.yaml
# Editar secrets.local.yaml con valores reales

# 2. Aplicar configuraciones
kubectl apply -f k8s/prod/base/namespace.yaml
kubectl apply -f k8s/prod/base/configmap.yaml
kubectl apply -f k8s/prod/base/secrets.local.yaml  # Archivo local, no commiteado
kubectl apply -f k8s/prod/network/
kubectl apply -f k8s/prod/kafka/
kubectl apply -f k8s/prod/microservices/
kubectl apply -f k8s/prod/ingress/
```

### Seguridad

#### NetworkPolicies
- **default-deny-ingress**: Deniega todo tráfico por defecto
- **allow-ingress-to-apis**: Solo el Ingress Controller puede acceder a las APIs
- **allow-apis-to-redpanda**: Solo las APIs pueden comunicarse con Redpanda
- **allow-api-to-api**: Comunicación interna entre microservicios

#### Autenticación Kafka/Redpanda
- **SASL/SCRAM-SHA-256** habilitado
- Credenciales almacenadas en Kubernetes Secrets

---

## 📋 Diferencias Local vs Producción

| Aspecto | Local | Producción |
|---------|-------|------------|
| PostgreSQL | StatefulSet en cluster | DigitalOcean Managed |
| Kafka | Redpanda en cluster | Redpanda DigitalOcean |
| Object Storage | MinIO | DigitalOcean Spaces |
| Ingress | NGINX local | NGINX + Cloudflare |
| TLS/SSL | No | Sí (Cloudflare) |
| NetworkPolicies | No | Sí |
| Secrets | Valores de desarrollo | ⚠️ Valores seguros |
| Replicas | 1 | 2-3 |
| Resources | Mínimos | Escalados |

---

## 🔧 Terraform

Para infraestructura como código, ver `/terraform/`:

```bash
cd terraform
terraform init
terraform plan -var-file="environments/production.tfvars"
terraform apply -var-file="environments/production.tfvars"
```
