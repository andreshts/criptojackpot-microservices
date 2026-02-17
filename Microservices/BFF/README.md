# BFF Gateway (Backend for Frontend)

API Gateway centralizado usando YARP (Yet Another Reverse Proxy) de Microsoft.

## Arquitectura

```
Frontend → Ingress → BFF Gateway (YARP) → Microservicios internos
                         ↓
                  Auth (Cookie HttpOnly)
                  Routing por recurso
                  Health checks
```

## Características

- **YARP Reverse Proxy**: Routing declarativo por configuración
- **Auth centralizada**: Validación de JWT desde Cookie HttpOnly
- **Health Checks**: Monitoreo de servicios downstream
- **CORS configurado**: Por entorno (local, qa, prod)

## Configuración de Rutas

Las rutas se definen en `appsettings.json`:

```json
{
  "ReverseProxy": {
    "Routes": {
      "identity-auth-route": {
        "ClusterId": "identity-cluster",
        "Match": { "Path": "/api/v1/auth/{**remainder}" }
      }
    },
    "Clusters": {
      "identity-cluster": {
        "Destinations": {
          "default": { "Address": "http://identity-api.cryptojackpot.svc.cluster.local" }
        }
      }
    }
  }
}
```

## Agregar una Nueva Ruta

1. Agregar route en `ReverseProxy.Routes`:
```json
"new-service-route": {
  "ClusterId": "new-cluster",
  "Match": { "Path": "/api/v1/newresource/{**remainder}" }
}
```

2. Agregar cluster en `ReverseProxy.Clusters`:
```json
"new-cluster": {
  "Destinations": {
    "default": { "Address": "http://new-service.cryptojackpot.svc.cluster.local" }
  }
}
```

## Endpoints Disponibles

| Recurso | Cluster | Servicio Backend |
|---------|---------|------------------|
| `/api/v1/auth/*` | identity-cluster | identity-api |
| `/api/v1/users/*` | identity-cluster | identity-api |
| `/api/v1/roles/*` | identity-cluster | identity-api |
| `/api/v1/countries/*` | identity-cluster | identity-api |
| `/api/v1/lotteries/*` | lottery-cluster | lottery-api |
| `/lottery-hub/*` | lottery-cluster | lottery-api (SignalR) |
| `/api/v1/orders/*` | order-cluster | order-api |
| `/api/v1/tickets/*` | order-cluster | order-api |
| `/api/v1/wallets/*` | wallet-cluster | wallet-api |
| `/api/v1/transactions/*` | wallet-cluster | wallet-api |
| `/api/v1/winners/*` | winner-cluster | winner-api |
| `/api/v1/notifications/*` | notification-cluster | notification-api |
| `/api/v1/audit/*` | audit-cluster | audit-api |

## Health Endpoints

- `GET /health` - Health check básico
- `GET /health/ready` - Readiness check (verifica servicios downstream)

## Desarrollo Local

El BFF está disponible en `http://localhost:8080` cuando se ejecuta con Skaffold:

```bash
skaffold dev --cleanup=false
```

### Proxy para Frontend (Vite)

```typescript
// vite.config.ts
export default defineConfig({
  server: {
    proxy: {
      '/api': 'http://localhost:8080',
      '/lottery-hub': { target: 'http://localhost:8080', ws: true }
    }
  }
});
```

## Autenticación

El BFF extrae el JWT desde cookies HttpOnly:

```csharp
options.Events = new JwtBearerEvents
{
    OnMessageReceived = context =>
    {
        if (context.Request.Cookies.TryGetValue("access_token", out var token))
            context.Token = token;
        return Task.CompletedTask;
    }
};
```

## Variables de Entorno

| Variable | Descripción | Default |
|----------|-------------|---------|
| `JwtSettings__SecretKey` | Clave secreta JWT | (requerido en prod) |
| `JwtSettings__Issuer` | Issuer del token | CryptoJackpot |
| `JwtSettings__Audience` | Audience del token | CryptoJackpotAPI |
| `CookieSettings__AccessTokenCookieName` | Nombre de cookie | access_token |
| `Cors__AllowedOrigins__0` | Origen permitido | http://localhost:3000 |

