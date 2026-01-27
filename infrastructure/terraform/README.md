# 🏗️ Terraform Infrastructure - CryptoJackpot

Este directorio contiene la configuración de **Infraestructura como Código (IaC)** usando Terraform para desplegar la plataforma CryptoJackpot en **DigitalOcean**.

## 📋 Tabla de Contenidos

- [Arquitectura](#-arquitectura)
- [Prerrequisitos](#-prerrequisitos)
- [Estructura de Archivos](#-estructura-de-archivos)
- [Configuración Inicial](#-configuración-inicial)
- [Uso](#-uso)
- [Módulos](#-módulos)
- [Variables](#-variables)
- [Outputs](#-outputs)
- [Integración con CI/CD](#-integración-con-cicd)
- [Solución de Problemas](#-solución-de-problemas)

## 🏛️ Arquitectura

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         DigitalOcean Cloud                              │
│  ┌───────────────────────────────────────────────────────────────────┐  │
│  │                          VPC (10.10.0.0/16)                       │  │
│  │                                                                   │  │
│  │  ┌─────────────────────────────────────────────────────────────┐  │  │
│  │  │                  DOKS (Kubernetes Cluster)                  │  │  │
│  │  │                                                             │  │  │
│  │  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐         │  │  │
│  │  │  │  Identity   │  │   Lottery   │  │    Order    │         │  │  │
│  │  │  │    API      │  │    API      │  │    API      │         │  │  │
│  │  │  └─────────────┘  └─────────────┘  └─────────────┘         │  │  │
│  │  │                                                             │  │  │
│  │  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐         │  │  │
│  │  │  │   Wallet    │  │   Winner    │  │ Notification│         │  │  │
│  │  │  │    API      │  │    API      │  │    API      │         │  │  │
│  │  │  └─────────────┘  └─────────────┘  └─────────────┘         │  │  │
│  │  │                                                             │  │  │
│  │  │  ┌─────────────────────────────────────────────────────┐   │  │  │
│  │  │  │                    Redpanda (Kafka)                 │   │  │  │
│  │  │  └─────────────────────────────────────────────────────┘   │  │  │
│  │  │                                                             │  │  │
│  │  │  ┌───────────────────┐   ┌───────────────────┐             │  │  │
│  │  │  │   NGINX Ingress   │   │   Cert-Manager    │             │  │  │
│  │  │  └───────────────────┘   └───────────────────┘             │  │  │
│  │  └─────────────────────────────────────────────────────────────┘  │  │
│  │                                                                   │  │
│  │  ┌─────────────────────────────────────────────────────────────┐  │  │
│  │  │              Managed PostgreSQL (6 Databases)               │  │  │
│  │  │  identity_db | lottery_db | order_db | wallet_db | ...      │  │  │
│  │  └─────────────────────────────────────────────────────────────┘  │  │
│  └───────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│  ┌─────────────────────┐  ┌─────────────────────┐                       │
│  │   Container Registry│  │   Spaces (S3)       │                       │
│  │   (DOCR)            │  │   Object Storage    │                       │
│  └─────────────────────┘  └─────────────────────┘                       │
└─────────────────────────────────────────────────────────────────────────┘
```

## 📦 Prerrequisitos

1. **Terraform** >= 1.5.0
   ```powershell
   # Windows (Chocolatey)
   choco install terraform
   
   # macOS (Homebrew)
   brew install terraform
   ```

2. **doctl** (DigitalOcean CLI)
   ```powershell
   # Windows (Chocolatey)
   choco install doctl
   
   # macOS (Homebrew)
   brew install doctl
   ```

3. **kubectl**
   ```powershell
   # Windows (Chocolatey)
   choco install kubernetes-cli
   ```

4. **Cuenta de DigitalOcean** con:
   - API Token ([Crear aquí](https://cloud.digitalocean.com/account/api/tokens))
   - Spaces Access Keys ([Crear aquí](https://cloud.digitalocean.com/account/api/spaces))

## 📁 Estructura de Archivos

```
terraform/
├── main.tf                    # Configuración principal - orquesta módulos
├── variables.tf               # Variables globales
├── outputs.tf                 # Outputs de la infraestructura
├── providers.tf               # Configuración de providers
├── versions.tf                # Versiones requeridas
├── terraform.tfvars.example   # Template de variables
├── .gitignore                 # Archivos a ignorar
│
├── environments/
│   ├── dev.tfvars             # Variables para desarrollo
│   └── prod.tfvars            # Variables para producción
│
├── modules/
│   ├── vpc/                   # Red privada virtual
│   ├── doks/                  # Kubernetes cluster
│   ├── docr/                  # Container Registry
│   ├── database/              # PostgreSQL managed
│   ├── spaces/                # Object storage
│   ├── secrets/               # Kubernetes secrets
│   └── ingress/               # NGINX Ingress + Cert-Manager
│
├── templates/
│   └── deploy-config.tpl      # Template para config de deploy
│
└── scripts/
    ├── post-apply.sh          # Script post-terraform (Linux/macOS)
    └── post-apply.ps1         # Script post-terraform (Windows)
```

## ⚙️ Configuración Inicial

### 1. Autenticar con DigitalOcean

```powershell
doctl auth init
# Ingresa tu API token cuando se solicite
```

### 2. Crear archivo de variables

```powershell
cd terraform
cp terraform.tfvars.example terraform.tfvars
```

### 3. Editar `terraform.tfvars`

```hcl
# Tokens de DigitalOcean (REQUERIDO)
do_token          = "dop_v1_tu_token_aqui"
spaces_access_key = "tu_access_key_aqui"
spaces_secret_key = "tu_secret_key_aqui"

# Configuración del proyecto
project_name = "cryptojackpot"
environment  = "prod"
region       = "nyc3"

# ... resto de configuración
```

### 4. Inicializar Terraform

```powershell
terraform init
```

## 🚀 Uso

### Despliegue Completo

```powershell
# Ver plan de cambios
terraform plan -var-file="environments/prod.tfvars"

# Aplicar cambios
terraform apply -var-file="environments/prod.tfvars"

# Ejecutar script post-apply
.\scripts\post-apply.ps1
```

### Despliegue por Ambiente

```powershell
# Desarrollo
terraform apply -var-file="environments/dev.tfvars"

# Producción
terraform apply -var-file="environments/prod.tfvars"
```

### Destruir Infraestructura

```powershell
# ⚠️ CUIDADO: Esto eliminará TODOS los recursos
terraform destroy -var-file="environments/prod.tfvars"
```

### Comandos Útiles

```powershell
# Ver estado actual
terraform show

# Ver outputs
terraform output

# Ver output específico (ej: kubeconfig)
terraform output -raw cluster_kubeconfig > kubeconfig.yaml

# Refrescar estado
terraform refresh

# Validar configuración
terraform validate

# Formatear archivos
terraform fmt -recursive
```

## 📦 Módulos

### VPC (`modules/vpc`)
Crea una red privada virtual para aislar los recursos.

```hcl
module "vpc" {
  source   = "./modules/vpc"
  name     = "cryptojackpot-vpc"
  region   = "nyc3"
  ip_range = "10.10.0.0/16"
}
```

### DOKS (`modules/doks`)
Despliega un cluster de Kubernetes managed.

| Variable | Descripción | Default |
|----------|-------------|---------|
| `node_size` | Tamaño de los nodos | `s-2vcpu-4gb` |
| `node_count` | Número de nodos | `3` |
| `auto_scale` | Habilitar auto-scaling | `true` |
| `min_nodes` | Mínimo de nodos | `2` |
| `max_nodes` | Máximo de nodos | `5` |

### Database (`modules/database`)
Crea PostgreSQL managed con las 6 bases de datos.

Bases de datos creadas:
- `cryptojackpot_identity_db`
- `cryptojackpot_lottery_db`
- `cryptojackpot_order_db`
- `cryptojackpot_wallet_db`
- `cryptojackpot_winner_db`
- `cryptojackpot_notification_db`

### DOCR (`modules/docr`)
Container Registry para imágenes Docker.

| Tier | Almacenamiento | Precio |
|------|----------------|--------|
| `starter` | 500MB | Gratis |
| `basic` | 5GB | $5/mes |
| `professional` | Ilimitado | $20/mes |

### Spaces (`modules/spaces`)
Object storage compatible con S3.

Directorios creados automáticamente:
- `profile-images/`
- `lottery-images/`
- `prize-images/`
- `documents/`

### Secrets (`modules/secrets`)
Genera automáticamente los Kubernetes secrets con valores reales.

### Ingress (`modules/ingress`)
Instala NGINX Ingress Controller y Cert-Manager.

## 📤 Variables Principales

| Variable | Descripción | Requerida |
|----------|-------------|-----------|
| `do_token` | Token API de DigitalOcean | ✅ |
| `spaces_access_key` | Access key de Spaces | ✅ |
| `spaces_secret_key` | Secret key de Spaces | ✅ |
| `project_name` | Nombre del proyecto | No |
| `environment` | Ambiente (dev/staging/prod) | No |
| `region` | Región de DO | No |

## 📥 Outputs Importantes

| Output | Descripción |
|--------|-------------|
| `cluster_endpoint` | URL del API server de K8s |
| `cluster_kubeconfig` | Kubeconfig completo |
| `registry_url` | URL del container registry |
| `database_host` | Host de PostgreSQL |
| `spaces_endpoint` | Endpoint de Spaces |
| `ingress_load_balancer_ip` | IP del Load Balancer |

```powershell
# Obtener kubeconfig
terraform output -raw cluster_kubeconfig > ~/.kube/config

# Obtener URL del registry
terraform output registry_url
# Output: registry.digitalocean.com/cryptojackpot
```

## 🔄 Integración con CI/CD

### GitHub Actions

```yaml
# .github/workflows/terraform.yml
name: 'Terraform'

on:
  push:
    branches: [ main ]
    paths: [ 'terraform/**' ]

jobs:
  terraform:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - uses: hashicorp/setup-terraform@v3
      with:
        terraform_version: 1.5.0
    
    - name: Terraform Init
      working-directory: terraform
      run: terraform init
      env:
        AWS_ACCESS_KEY_ID: ${{ secrets.DO_SPACES_ACCESS_KEY }}
        AWS_SECRET_ACCESS_KEY: ${{ secrets.DO_SPACES_SECRET_KEY }}
    
    - name: Terraform Plan
      working-directory: terraform
      run: terraform plan -var-file="environments/prod.tfvars"
      env:
        TF_VAR_do_token: ${{ secrets.DO_TOKEN }}
        TF_VAR_spaces_access_key: ${{ secrets.DO_SPACES_ACCESS_KEY }}
        TF_VAR_spaces_secret_key: ${{ secrets.DO_SPACES_SECRET_KEY }}
```

### Backend Remoto (Recomendado para equipos)

Descomentar en `versions.tf`:

```hcl
backend "s3" {
  endpoint                    = "nyc3.digitaloceanspaces.com"
  bucket                      = "cryptojackpot-terraform-state"
  key                         = "terraform.tfstate"
  region                      = "us-east-1"
  skip_credentials_validation = true
  skip_metadata_api_check     = true
}
```

## 🔧 Solución de Problemas

### Error: "unauthorized"
```
Verificar que do_token es válido y tiene permisos de escritura
```

### Error: "database cluster not ready"
```powershell
# La base de datos tarda ~5 minutos en estar lista
# Esperar y reintentar
terraform apply
```

### Error: "ingress load balancer pending"
```powershell
# El Load Balancer tarda ~2-3 minutos
kubectl get svc -n ingress-nginx -w
```

### Resetear estado corrupto
```powershell
# Mover estado y reiniciar
mv terraform.tfstate terraform.tfstate.backup
terraform import module.vpc.digitalocean_vpc.main <vpc-id>
# ... importar otros recursos
```

## 📊 Costos Estimados (DigitalOcean)

| Recurso | Configuración Dev | Configuración Prod |
|---------|-------------------|-------------------|
| DOKS (2-3 nodos) | ~$24-48/mes | ~$96-192/mes |
| PostgreSQL | ~$15/mes | ~$60/mes (HA) |
| Spaces | ~$5/mes | ~$10/mes |
| Load Balancer | ~$12/mes | ~$12/mes |
| Registry | Gratis-$5/mes | $20/mes |
| **Total** | **~$56-70/mes** | **~$198-294/mes** |

## 🔒 Seguridad

- ✅ VPC aislada para todos los recursos
- ✅ PostgreSQL solo accesible desde el cluster K8s
- ✅ Secrets generados automáticamente
- ✅ SSL/TLS con Let's Encrypt
- ✅ Network Policies en Kubernetes
- ✅ SASL/SCRAM para Kafka

## 📚 Referencias

- [Terraform DigitalOcean Provider](https://registry.terraform.io/providers/digitalocean/digitalocean/latest/docs)
- [DigitalOcean Kubernetes](https://docs.digitalocean.com/products/kubernetes/)
- [DigitalOcean Managed Databases](https://docs.digitalocean.com/products/databases/)
- [DigitalOcean Spaces](https://docs.digitalocean.com/products/spaces/)

