# Despliegue del backend en Azure

Arquitectura: **Azure App Service (contenedor Linux)** + **Azure Container Registry** + **Azure Database for PostgreSQL (Flexible Server)** + **RabbitMQ en CloudAMQP (plan gratis)**. El despliegue lo hace GitHub Actions cada vez que se hace push a la rama `deploy`.

```
push a "deploy" -> GitHub Actions: compila, construye la imagen, la sube a ACR
                -> App Service descarga la imagen y arranca el contenedor (puerto 8080)
                -> el backend aplica las migraciones en PostgreSQL al iniciar
```

Costo aproximado al mes: App Service B1 ~13 USD, PostgreSQL B1ms ~12–15 USD, ACR Basic ~5 USD, CloudAMQP 0 USD. Con **Azure for Students** (100 USD de crédito) alcanza para el semestre; en esa suscripción solo se pueden usar algunas regiones, así que si un comando falla por la región, prueba `eastus`, `eastus2` o `centralus`.

## 0. Requisitos
- Cuenta de Azure y la [CLI de Azure](https://learn.microsoft.com/cli/azure/install-azure-cli) (`az`).
- Cuenta en [cloudamqp.com](https://www.cloudamqp.com) (plan *Little Lemur*, gratis).
- Permisos de administrador en el repo de GitHub (para crear *secrets*).

## 1. Recursos en Azure
En PowerShell (cambia los nombres marcados; los de ACR y App Service deben ser únicos en todo Azure, en minúsculas y sin guiones el de ACR):

```powershell
az login

$RG    = "rg-routeguard"
$LOC   = "eastus2"
$ACR   = "routeguardacr<tus-iniciales>"      # solo letras y números
$PLAN  = "plan-routeguard"
$APP   = "routeguard-api-<tus-iniciales>"
$PG = "pg-routeguard-<tus-iniciales>"
$DBUSER = "routeguardadmin"
$DBPASS = "<clave-larga-con-mayusculas-numeros-y-simbolos>"

az group create --name $RG --location $LOC

# Registro de contenedores
az acr create --resource-group $RG --name $ACR --sku Basic --admin-enabled true
az acr credential show --name $ACR        # usuario y contraseña para GitHub

# Base de datos PostgreSQL 16
az postgres flexible-server create --resource-group $RG --name $PG --location $LOC `
  --admin-user $DBUSER --admin-password $DBPASS `
  --sku-name Standard_B1ms --tier Burstable --storage-size 32 --version 16 `
  --public-access 0.0.0.0 --yes
az postgres flexible-server db create --resource-group $RG --server-name $PG --database-name routeguard

# App Service con contenedor
az appservice plan create --name $PLAN --resource-group $RG --is-linux --sku B1
az webapp create --resource-group $RG --plan $PLAN --name $APP `
  --container-image-name "$ACR.azurecr.io/routeguard-platform-wa:latest" `
  --container-registry-url "https://$ACR.azurecr.io" `
  --container-registry-user <usuario-de-acr> --container-registry-password <contraseña-de-acr>
az webapp update --resource-group $RG --name $APP --https-only true
```

`--public-access 0.0.0.0` permite que los servicios de Azure (tu App Service) lleguen a PostgreSQL. Para abrir pgAdmin o DBeaver desde tu PC agrega tu IP: `az postgres flexible-server firewall-rule create --resource-group $RG --name $PG --rule-name mi-pc --start-ip-address <tu-ip> --end-ip-address <tu-ip>`.

## 2. RabbitMQ (CloudAMQP)
1. Crea una instancia gratis y copia la **AMQP URL** (`amqps://usuario:clave@host/vhost`).

## 3. Configuración de la app
Son las variables de entorno que lee el backend (ver `docs/seguridad-y-configuracion.md`). Genera la clave del JWT con al menos 32 caracteres aleatorios.

```powershell
az webapp config appsettings set --resource-group $RG --name $APP --settings `
  WEBSITES_PORT=8080 `
  ASPNETCORE_ENVIRONMENT=Production `
  JWT_SECRET="<clave-aleatoria-de-32-o-mas-caracteres>" `
  DATABASE_URL="$PG.postgres.database.azure.com" `
  DATABASE_PORT=5432 `
  DATABASE_SCHEMA=routeguard `
  DATABASE_USER=$DBUSER `
  DATABASE_PASSWORD=$DBPASS `
  RABBITMQ_URL="<amqp-url-de-cloudamqp>" `
  Seed__Enabled=false `
  Cors__AllowedOrigins__0="https://upc-pre-202620-1acc0238-4945-routegolem.github.io" `
  HardwareAdapter__ApiKey="<clave-para-el-hardware>"
```

- `Seed__Enabled=true` crea los usuarios de demostración (`admin@routeguard.pe` / `admin123`…). Solo para una demo; con claves conocidas no debe quedar en un entorno real.
- Activa el *health check*: portal de Azure → tu App Service → **Monitoring → Health check** → ruta `/health`.
- Logs en vivo: `az webapp log config --resource-group $RG --name $APP --docker-container-logging filesystem` y luego `az webapp log tail --resource-group $RG --name $APP`.

## 4. GitHub Actions
El workflow `.github/workflows/deploy.yml` necesita estos *secrets* (repo → Settings → Secrets and variables → Actions):

| Secret | Valor |
|---|---|
| `ACR_LOGIN_SERVER` | `<ACR>.azurecr.io` |
| `ACR_USERNAME` / `ACR_PASSWORD` | de `az acr credential show` |
| `AZURE_WEBAPP_NAME` | el nombre `$APP` |
| `AZURE_WEBAPP_PUBLISH_PROFILE` | contenido del perfil de publicación (comando de abajo) |

Los perfiles de publicación requieren la autenticación básica del sitio SCM, que Azure desactiva en apps nuevas:

```powershell
az resource update --resource-group $RG --name scm --namespace Microsoft.Web `
  --resource-type basicPublishingCredentialsPolicies --parent "sites/$APP" --set properties.allow=true
az webapp deployment list-publishing-profiles --name $APP --resource-group $RG --xml
```

## 5. Desplegar
```bash
git push origin main:deploy
```
En la pestaña **Actions** se ve el avance: compila, construye la imagen, la sube a ACR, actualiza el App Service y al final comprueba `GET /health`. También se puede lanzar a mano con *Run workflow*.

## 6. Verificar
```powershell
$HOST_ = az webapp show --resource-group $RG --name $APP --query defaultHostName -o tsv
curl "https://$HOST_/health"        # debe responder: Healthy
```
Swagger solo está activo en desarrollo. Con `Seed__Enabled=false` no hay usuarios; crea el primer administrador:

```powershell
$org = curl -s -X POST "https://$HOST_/api/v1/organizations" -H "Content-Type: application/json" -d '{"name":"Mi empresa"}' | ConvertFrom-Json
curl -X POST "https://$HOST_/api/v1/users" -H "Content-Type: application/json" `
  -d "{`"firstName`":`"Admin`",`"lastName`":`"Principal`",`"email`":`"admin@miempresa.pe`",`"password`":`"<clave>`",`"roleTier`":`"ADMIN`",`"organizationId`":`"$($org.id)`"}"
```
(también se puede hacer desde la app con "Registrar administrador").

## 7. La app Android
Hoy `BASE_URL` apunta a `localhost`. Para usar el servidor desplegado hace falta una URL por tipo de compilación (`https://<HOST>/api/v1/` en *release*) y dejar `usesCleartextTraffic` solo para *debug*.

## 8. Problemas típicos
- **El contenedor no arranca / "Application Error":** `az webapp log tail`. Casi siempre falta `JWT_SECRET` (el backend se niega a arrancar si tiene menos de 32 caracteres) o `WEBSITES_PORT=8080`.
- **Error de conexión a PostgreSQL:** revisa `DATABASE_*` y que la regla `0.0.0.0` exista. La conexión exige SSL.
- **`Resource not accessible` / el workflow no corre:** revisa que los 5 secrets existan y que el perfil de publicación sea el XML completo.
- **El push a `deploy` no dispara nada:** el workflow debe estar en esa rama; haz `git push origin main:deploy` con el workflow ya subido a `main`.

## 9. Borrar todo (para no gastar crédito)
```powershell
az group delete --name $RG --yes --no-wait
```
