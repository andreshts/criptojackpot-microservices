# Kubernetes Deployment - CryptoJackpotDistributed

## Arquitectura de Base de Datos

Usamos **una instancia de PostgreSQL Managed** en DigitalOcean con **6 databases separadas** (una por microservicio).

```
PostgreSQL Server (DigitalOcean Managed)
├── cryptojackpot_identity_db
├── cryptojackpot_lottery_db
├── cryptojackpot_order_db
├── cryptojackpot_wallet_db
├── cryptojackpot_winner_db
└── cryptojackpot_notification_db
```

## Estructura de Archivos

```
k8s/
├── base/                    # Configuraciones base
│   ├── namespace.yaml
│   ├── configmap.yaml
│   └── secrets.yaml
├── databases/               # Scripts de inicialización
│   └── init-databases.sql
├── network/                 # Seguridad de red
│   └── network-policies.yaml
├── microservices/           # Deployments por servicio
│   ├── identity/
│   ├── lottery/
│   ├── order/
│   ├── wallet/
│   ├── winner/
│   └── notification/
├── ingress/                 # Configuración de Ingress
│   └── ingress.yaml
└── kafka/                   # Kafka/Redpanda con SASL
    └── redpanda.yaml
```

## Seguridad Implementada

### NetworkPolicies
- **default-deny-ingress**: Deniega todo tráfico por defecto
- **allow-ingress-to-apis**: Solo el Ingress Controller puede acceder a las APIs
- **allow-apis-to-redpanda**: Solo las APIs pueden comunicarse con Redpanda
- **allow-api-to-api**: Comunicación interna entre microservicios

### Autenticación Kafka/Redpanda
- **SASL/SCRAM-SHA-256**: Autenticación habilitada
- Credenciales almacenadas en Kubernetes Secrets

## Despliegue Rápido

1. Crear cluster en DigitalOcean
2. Crear PostgreSQL Managed
3. Ejecutar script de inicialización de databases
4. Aplicar configuraciones de Kubernetes

```bash
kubectl apply -f k8s/base/
kubectl apply -f k8s/network/
kubectl apply -f k8s/kafka/
kubectl apply -f k8s/microservices/
kubectl apply -f k8s/ingress/
```

