# Terraform - CriptoJackpot Infrastructure (DigitalOcean)

Gestiona toda la infraestructura de QA y Producción en DigitalOcean via Terraform.

## Arquitectura desplegada

```
Cloudflare (DNS + TLS)
        │
        ▼
DigitalOcean Load Balancer  ◄── creado automáticamente por DOKS
        │
        ▼
NGINX Ingress Controller (DOKS)
        │
        ▼
BFF Gateway (ClusterIP)  ◄── único punto de entrada al cluster
        │
   ┌────┴─────────────────────┐
   ▼                          ▼
Microservicios          Servicios externos
(ClusterIP)             ┌─ Upstash Kafka  (SASL_SSL)
                        ├─ Upstash Redis  (TLS)
                        ├─ MongoDB Atlas  (audit)
                        └─ Brevo          (emails)
        │
        ▼
PgBouncer (ClusterIP)
        │
        ▼
DO Managed PostgreSQL (VPC privada)
```

## Recursos gestionados por Terraform

| Recurso | QA | Prod |
|---------|----|----|
| VPC (privada) | `criptojackpot-qa-vpc` | `criptojackpot-prod-vpc` |
| DOKS Cluster | `criptojackpot-qa-cluster` | `criptojackpot-prod-cluster` |
| DO Managed PostgreSQL | Standalone (1 nodo) | HA (2 nodos) |
| DO Container Registry | Compartido (`criptojackpot`) | Compartido |
| DO Spaces (Object Storage) | `criptojackpot-qa-assets` | `criptojackpot-prod-assets` |
| NGINX Ingress (Helm) | 1 réplica | 2 réplicas |
| K8s Secrets | 7 secrets | 7 secrets |
| Cloudflare DNS | `api-qa.criptojackpot.com` → LB IP | `api.criptojackpot.com` → LB IP |
| Kustomize apply | `overlays/qa` | `overlays/prod` |

**No gestionado por Terraform** (externos, solo credenciales via variables):
- Upstash Kafka, Upstash Redis, MongoDB Atlas, Brevo

## Estructura

```
terraform/
├── main.tf                    # Orquestación principal
├── variables.tf               # Todas las variables
├── outputs.tf                 # Outputs útiles (IPs, comandos)
├── providers.tf               # DO, Kubernetes, Helm, Cloudflare
├── versions.tf                # Versiones + backend remoto (DO Spaces)
├── terraform.tfvars.example   # Plantilla de variables sensibles
├── environments/
│   ├── qa.tfvars              # Variables de QA
│   └── prod.tfvars            # Variables de Producción
└── modules/
    ├── vpc/                   # VPC privada
    ├── doks/                  # Cluster Kubernetes (DOKS)
    ├── docr/                  # Container Registry
    ├── database/              # DO Managed PostgreSQL
    ├── spaces/                # DO Spaces (Object Storage)
    ├── ingress/               # NGINX Ingress via Helm
    └── secrets/               # K8s Secrets con valores reales
```

## Prerequisitos

```powershell
# Instalar herramientas
# - Terraform >= 1.7.0
# - doctl (DigitalOcean CLI)
# - kubectl
# - Autenticarse con doctl
doctl auth init
```

## Setup inicial (una sola vez)

```powershell
# 1. Crear el Space para el estado remoto de Terraform
doctl spaces create criptojackpot-tf-state --region nyc3

# 2. Copiar la plantilla de variables
Copy-Item terraform.tfvars.example terraform.tfvars
# Editar terraform.tfvars con los valores reales (nunca commitear este archivo)
```

## Despliegue QA

```powershell
# Variables de entorno con secrets (en CI/CD estas van como secrets del repo)
$env:TF_VAR_do_token              = "dop_v1_..."
$env:TF_VAR_spaces_access_key     = "..."
$env:TF_VAR_spaces_secret_key     = "..."
$env:TF_VAR_cloudflare_api_token  = "..."
$env:TF_VAR_cloudflare_zone_id    = "..."
$env:TF_VAR_kafka_bootstrap_servers = "your-cluster.upstash.io:9092"
$env:TF_VAR_kafka_sasl_username   = "..."
$env:TF_VAR_kafka_sasl_password   = "..."
$env:TF_VAR_redis_connection_string = "your-redis.upstash.io:6379,password=...,ssl=True"
$env:TF_VAR_mongodb_connection_string = "mongodb+srv://..."
$env:TF_VAR_brevo_api_key         = "xkeysib-..."

# Backend con state separado para QA
$env:AWS_ACCESS_KEY_ID     = $env:TF_VAR_spaces_access_key
$env:AWS_SECRET_ACCESS_KEY = $env:TF_VAR_spaces_secret_key

terraform init -backend-config="key=qa/terraform.tfstate"
terraform plan  -var-file="environments/qa.tfvars"
terraform apply -var-file="environments/qa.tfvars"
```

## Despliegue Producción

```powershell
# (mismas variables de entorno que QA)

terraform init -backend-config="key=prod/terraform.tfstate" -reconfigure
terraform plan  -var-file="environments/prod.tfvars"
terraform apply -var-file="environments/prod.tfvars"
```

## Qué hace `terraform apply`

1. **VPC** → Crea red privada aislada
2. **DOKS** → Cluster Kubernetes en la VPC
3. **DOCR** → Container Registry (compartido QA/prod, tags diferentes)
4. **PostgreSQL** → DO Managed DB, crea las 6 bases de datos, firewall solo-cluster
5. **Spaces** → Bucket privado para assets con CORS configurado
6. **NGINX Ingress** → Helm release, crea el Load Balancer de DO automáticamente
7. **K8s Secrets** → Crea los 7 secrets en el namespace `criptojackpot` con valores reales
8. **Kustomize apply** → Despliega todos los microservicios del overlay correcto (`qa`/`prod`)
9. **Cloudflare DNS** → Crea el registro `A` apuntando al LB IP con proxy ON

## Secrets creados en Kubernetes

| Secret K8s | Contenido |
|-----------|-----------|
| `postgres-secrets` | Host DO, port, user, password, 6 connection strings via PgBouncer |
| `jwt-secrets` | JWT key, issuer, audience |
| `kafka-secrets` | Upstash bootstrap servers, SASL user/pass, SCRAM-SHA-256, SASL_SSL |
| `redis-secrets` | Upstash Redis connection string |
| `mongodb-secrets` | MongoDB Atlas connection string + database name |
| `digitalocean-spaces-secrets` | Endpoint, bucket, access/secret key |
| `brevo-secrets` | API key, sender email/name, frontend base URL |

## Outputs útiles

```powershell
# Ver todos los outputs
terraform output

# Conectar kubectl al cluster
terraform output -raw cmd_kubectl_connect | Invoke-Expression

# IP del Load Balancer (para verificar DNS en Cloudflare)
terraform output ingress_load_balancer_ip

# Comando para desplegar imágenes
terraform output cmd_build_push_images
```

## Destroy (con cuidado)

```powershell
# QA - seguro destruir
terraform destroy -var-file="environments/qa.tfvars"

# Prod - NUNCA sin aprobación explícita
# spaces_force_destroy = false protege el bucket de datos de usuarios
```

## CI/CD (GitHub Actions)

El flujo recomendado es:

```
push → main  →  build images → push to DOCR → terraform apply prod
push → develop → build images → push to DOCR → terraform apply qa
```

Los secrets `TF_VAR_*` se configuran como **Repository Secrets** en GitHub Actions.
