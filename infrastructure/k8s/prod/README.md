# Producción - Kubernetes Configuration

## ⚠️ IMPORTANTE

Este directorio contiene las configuraciones de Kubernetes para **PRODUCCIÓN**.

### Archivos Sensibles

Los siguientes archivos contienen **plantillas** con valores de ejemplo:
- `base/secrets.yaml` - Contiene placeholders, NO valores reales

### Antes de Desplegar

1. **NUNCA** commitear secrets con valores reales
2. Crear copia local de secrets:
   ```bash
   cp base/secrets.yaml base/secrets.local.yaml
   ```
3. Editar `secrets.local.yaml` con valores reales
4. Agregar `*.local.yaml` a `.gitignore`

## Despliegue

```bash
# Conectar al cluster de producción
doctl kubernetes cluster kubeconfig save your-cluster-name

# Verificar contexto
kubectl config current-context

# Aplicar en orden
kubectl apply -f base/namespace.yaml
kubectl apply -f base/configmap.yaml
kubectl apply -f base/secrets.local.yaml
kubectl apply -f network/
kubectl apply -f kafka/
kubectl apply -f microservices/identity/
kubectl apply -f microservices/lottery/
kubectl apply -f microservices/order/
kubectl apply -f microservices/wallet/
kubectl apply -f microservices/winner/
kubectl apply -f microservices/notification/
kubectl apply -f ingress/
```

## Estructura

```
prod/
├── base/
│   ├── namespace.yaml      # Namespace cryptojackpot
│   ├── configmap.yaml      # Configuración de aplicación
│   └── secrets.yaml        # ⚠️ Plantilla de secrets
├── databases/
│   └── init-databases.sql  # Script para crear DBs en PostgreSQL
├── network/
│   └── network-policies.yaml  # Políticas de red (seguridad)
├── kafka/
│   └── redpanda.yaml       # Config de Redpanda con SASL
├── microservices/
│   ├── identity/
│   ├── lottery/
│   ├── order/
│   ├── wallet/
│   ├── winner/
│   └── notification/
└── ingress/
    └── ingress.yaml        # NGINX Ingress con rutas
```

## Verificación

```bash
# Ver todos los recursos
kubectl get all -n cryptojackpot

# Ver pods en estado
kubectl get pods -n cryptojackpot -w

# Ver logs
kubectl logs -f deployment/identity-api -n cryptojackpot

# Describir un pod con problemas
kubectl describe pod <pod-name> -n cryptojackpot
```

## Rollback

```bash
# Ver historial de deployment
kubectl rollout history deployment/identity-api -n cryptojackpot

# Rollback a versión anterior
kubectl rollout undo deployment/identity-api -n cryptojackpot

# Rollback a versión específica
kubectl rollout undo deployment/identity-api --to-revision=2 -n cryptojackpot
```
