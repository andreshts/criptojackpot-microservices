# Keycloak Integration Guide

This document describes the Keycloak OIDC integration for CryptoJackpot microservices.

## Overview

Keycloak provides centralized identity management with support for:
- **OAuth 2.0 / OpenID Connect** authentication
- **Social Login** (Google, Facebook, GitHub, etc.)
- **Multi-Factor Authentication (MFA/2FA)** with TOTP
- **Single Sign-On (SSO)** across applications
- **Session Management** and centralized logout

## Resource Requirements

### Development
- **Single replica**: 512MB - 1GB RAM
- Kubernetes NodePort: 30180

### Production (High Availability)
- **2 replicas**: ~2GB RAM total (1GB each)
- With HPA: can scale to 4 replicas under load

## Development Setup

### Option 1: Kubernetes Local (Recommended)

```powershell
# From infrastructure/k8s/local directory
.\setup-local-k8s.ps1
```

This will deploy all infrastructure including Keycloak. Access:
- **Keycloak Admin Console**: http://localhost:30180
- **Username**: `admin`
- **Password**: `admin`

### Option 2: Docker Compose

```powershell
# From project root
docker-compose -f docker-compose.infra.yaml up -d
```

Access Keycloak at http://localhost:8180

### Verify Realm Import

The `cryptojackpot` realm is automatically imported with:
- **Clients**: `cryptojackpot-backend` (confidential), `cryptojackpot-frontend` (public)
- **Roles**: `admin`, `moderator`, `user`
- **Identity Providers**: Google (requires configuration)

### Configure Google Social Login (Optional)

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create OAuth 2.0 credentials
3. In Keycloak Admin Console:
   - Navigate to: Identity Providers > Google
   - Enter Client ID and Client Secret
   - Save

## Configuration

### Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `Keycloak__Authority` | Keycloak base URL | `http://keycloak:8080` |
| `Keycloak__Realm` | Realm name | `cryptojackpot` |
| `Keycloak__ClientId` | Client ID | `cryptojackpot-backend` |
| `Keycloak__ClientSecret` | Client secret | `your-secret` |
| `Keycloak__RequireHttpsMetadata` | Require HTTPS | `false` for dev |
| `Keycloak__ValidateAudience` | Validate audience | `false` for dev |

## Production Deployment

### Kubernetes

```powershell
# Apply Keycloak resources
kubectl apply -f infrastructure/k8s/prod/keycloak/

# Verify deployment
kubectl get pods -n cryptojackpot -l app=keycloak
```

### Required Secrets (Production)

```powershell
kubectl create secret generic keycloak-secrets -n cryptojackpot \
  --from-literal=KEYCLOAK_ADMIN=admin \
  --from-literal=KEYCLOAK_ADMIN_PASSWORD=your-secure-password \
  --from-literal=KC_DB_URL=jdbc:postgresql://your-db:25060/cryptojackpot_keycloak_db?sslmode=require \
  --from-literal=KC_DB_USERNAME=doadmin \
  --from-literal=KC_DB_PASSWORD=your-db-password
```

## Token Claims

Keycloak tokens include:

| Claim | Description |
|-------|-------------|
| `sub` | Keycloak user ID (UUID) |
| `user_id` | Application user ID |
| `email` | User's email |
| `roles` | Array of realm roles |

### Accessing Claims in Code

```csharp
using CryptoJackpot.Infra.IoC;

[Authorize]
public class MyController : ControllerBase
{
    [HttpGet]
    public IActionResult GetUserInfo()
    {
        var userId = User.GetUserId();
        var email = User.GetEmail();
        var isAdmin = User.HasRole("admin");
        return Ok(new { userId, email, isAdmin });
    }
}
```

## Files Reference

| File | Purpose |
|------|---------|
| `infrastructure/docker/keycloak/realm-export.json` | Realm config for Docker |
| `infrastructure/k8s/local/keycloak/` | Local Kubernetes manifests |
| `infrastructure/k8s/prod/keycloak/` | Production Kubernetes manifests |
| `Infra.IoC/KeycloakAuthenticationExtensions.cs` | Shared auth config |
| `Identity/Application/Services/KeycloakUserService.cs` | User CRUD operations |
| `Identity/Application/Services/KeycloakRoleService.cs` | Role management |
| `Identity/Application/Services/KeycloakTokenService.cs` | Token operations (login, refresh) |
| `Identity/Application/Http/KeycloakAdminTokenHandler.cs` | Admin token management (DelegatingHandler) |
| `Identity/Application/Http/KeycloakEndpoints.cs` | API endpoint constants |

## Troubleshooting

### Token Validation Fails
1. Check `Keycloak__Authority` is reachable
2. Verify `RequireHttpsMetadata` is `false` for HTTP
3. Check Keycloak logs: `kubectl logs -l app=keycloak -n cryptojackpot`

### Keycloak Not Starting
1. Check PostgreSQL is ready first
2. Keycloak needs 1-2 minutes to start (JVM warmup)
3. Check health: `kubectl get pods -n cryptojackpot -l app=keycloak`
