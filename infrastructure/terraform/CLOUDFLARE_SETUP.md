# Configuración de Cloudflare con Terraform

Este documento explica cómo configurar la automatización DNS de Cloudflare para tu infraestructura CryptoJackpot.

## Objetivo

Automatizar la creación del registro DNS en Cloudflare que apunta al Load Balancer de NGINX Ingress en DigitalOcean Kubernetes (DOKS).

## Pre-requisitos

1. **Dominio configurado en Cloudflare**: Tu dominio debe estar activo en Cloudflare
2. **Token de API de Cloudflare**: Con permisos de edición DNS
3. **Zone ID de Cloudflare**: ID único de tu zona/dominio

## Paso 1: Obtener credenciales de Cloudflare

### API Token

1. Ve a [Cloudflare API Tokens](https://dash.cloudflare.com/profile/api-tokens)
2. Click en "Create Token"
3. Usa la plantilla "Edit zone DNS" o crea uno personalizado con:
   - **Zone** → **DNS** → **Edit**
   - **Zone** → **Zone** → **Read** (opcional, para listar zonas)
4. Selecciona las zonas específicas o "All zones"
5. Guarda el token generado (solo se muestra una vez)

### Zone ID

1. Ve al dashboard de Cloudflare
2. Selecciona tu dominio (ej: `cryptojackpot.com`)
3. En la página principal, busca "Zone ID" en el panel derecho
4. Copia el valor (32 caracteres hexadecimales)

## Paso 2: Configurar las variables

### Opción A: Variables de entorno (recomendado para CI/CD)

```powershell
# PowerShell
$env:TF_VAR_cloudflare_api_token = "tu_token_aqui"
$env:TF_VAR_cloudflare_zone_id = "tu_zone_id_aqui"
```

```bash
# Bash
export TF_VAR_cloudflare_api_token="tu_token_aqui"
export TF_VAR_cloudflare_zone_id="tu_zone_id_aqui"
```

### Opción B: Archivo terraform.tfvars (para desarrollo local)

Crea o edita `terraform/terraform.tfvars`:

```hcl
cloudflare_api_token = "tu_token_aqui"
cloudflare_zone_id   = "tu_zone_id_aqui"
```

⚠️ **IMPORTANTE**: Nunca subas este archivo a Git. Ya está en `.gitignore`.

## Paso 3: Habilitar Cloudflare DNS

En tu archivo de entorno (`environments/dev.tfvars` o `environments/prod.tfvars`):

```hcl
enable_cloudflare_dns = true
cloudflare_proxied    = true  # false para desarrollo
```

## Paso 4: Aplicar los cambios

```bash
# Inicializar (descarga el provider de Cloudflare)
cd terraform
terraform init

# Ver el plan
terraform plan -var-file="environments/prod.tfvars"

# Aplicar
terraform apply -var-file="environments/prod.tfvars"
```

## Comportamiento esperado

1. Terraform crea toda la infraestructura en DigitalOcean
2. El Load Balancer de NGINX Ingress recibe una IP pública
3. Terraform crea automáticamente un registro DNS tipo A en Cloudflare:
   - `api.cryptojackpot.com` → IP del Load Balancer (producción)
   - `dev-api.cryptojackpot.com` → IP del Load Balancer (desarrollo)

## Nota sobre la primera ejecución

Es posible que en la primera ejecución el registro DNS no se cree porque:
- DigitalOcean tarda 1-3 minutos en asignar la IP al Load Balancer
- Terraform verá el valor como "pending"

**Solución**: Simplemente ejecuta `terraform apply` de nuevo después de 2-3 minutos.

## Configuración SSL/TLS en Cloudflare

Cuando `cloudflare_proxied = true`:

1. Ve a tu dashboard de Cloudflare → SSL/TLS → Overview
2. Configura el modo SSL como **"Full (Strict)"**

### ¿Por qué "Full (Strict)"?

- Tu cluster tiene cert-manager que genera certificados válidos de Let's Encrypt
- Cloudflare validará este certificado
- La conexión es segura de extremo a extremo:
  ```
  Usuario → HTTPS → Cloudflare → HTTPS → Load Balancer → cert-manager certificate
  ```

## Solución de problemas

### Los certificados de Let's Encrypt no se emiten

Si cert-manager falla al obtener certificados con el proxy de Cloudflare activo:

1. **Verifica Bot Fight Mode**: 
   - Cloudflare → Security → Bots → Bot Fight Mode → Desactivar temporalmente
   
2. **Verifica WAF Rules**:
   - Asegúrate de que no bloqueen `.well-known/acme-challenge`

3. **Usa DNS-01 challenge** (alternativa):
   - Más complejo pero funciona mejor con proxies

### El registro DNS no se crea

```bash
# Verifica que la IP no esté pendiente
terraform output ingress_load_balancer_ip

# Si muestra "pending", espera y re-aplica
terraform apply -var-file="environments/prod.tfvars"
```

### Error de autenticación de Cloudflare

```bash
# Verifica el token
curl -X GET "https://api.cloudflare.com/client/v4/user/tokens/verify" \
     -H "Authorization: Bearer TU_TOKEN_AQUI" \
     -H "Content-Type: application/json"
```

## Variables disponibles

| Variable | Tipo | Default | Descripción |
|----------|------|---------|-------------|
| `enable_cloudflare_dns` | bool | `false` | Habilita la creación de registros DNS |
| `cloudflare_api_token` | string | `""` | Token de API de Cloudflare |
| `cloudflare_zone_id` | string | `""` | Zone ID del dominio |
| `cloudflare_proxied` | bool | `true` | Activa proxy CDN/WAF (nube naranja) |

## Outputs

Después de aplicar, puedes ver:

```bash
terraform output cloudflare_dns_record
terraform output cloudflare_dns_hostname
```

## Recursos adicionales

- [Documentación del Provider de Cloudflare](https://registry.terraform.io/providers/cloudflare/cloudflare/latest/docs)
- [API de Cloudflare](https://developers.cloudflare.com/api/)
- [SSL/TLS en Cloudflare](https://developers.cloudflare.com/ssl/)

