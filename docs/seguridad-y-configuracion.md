# Seguridad y configuración del backend

## 1. Autenticación y roles

Toda la API exige un JWT (`Authorization: Bearer <token>`), salvo estas rutas, que son públicas:

| Ruta | Por qué |
|---|---|
| `POST /api/v1/users/sign-in` | Iniciar sesión |
| `POST /api/v1/organizations` | Registro de una empresa de transporte |
| `POST /api/v1/users` | Solo el **primer ADMIN** de una organización recién creada. Cualquier otro usuario lo crea un ADMIN de esa organización |
| `POST /api/v1/vehicle-locations` | Adaptador de hardware: usa la cabecera `X-Api-Key` (ver más abajo) |

Los roles salen del token (`ADMIN`, `DRIVER`, `PARENT`). Quién puede qué:

| Recurso | ADMIN | DRIVER | PARENT |
|---|---|---|---|
| Rutas, vehículos, grupos, perfiles, suscripciones, conductores y padres (crear/editar/borrar) | sí | no | no |
| `GET /routes`, `GET /routes/{id}` | sí | sí | no |
| `GET /drivers` | todos | solo su ficha | no |
| `GET /parents` | todos | no | solo su ficha |
| Viajes (`/trips`, abordaje, incidencias, finalizar), ubicaciones, offline-sync, pánico y avisos | sí | solo **sus** viajes | no |
| `GET /trips/live` | sí | no | no |
| `GET /parents/{id}/active-trip` | sí | no | solo el suyo |
| `GET /notifications` | todas | todas (pendiente acotar) | solo las suyas |
| `GET /plans` | sí | sí | sí |
| Tokens de dispositivo (`/users/{id}/device-tokens`) | sí | solo los suyos | solo los suyos |

Un `401` significa "sin sesión o token inválido/vencido" y un `403` "con sesión pero sin permiso".
La app Android cierra la sesión sola cuando recibe un `401`.

## 2. Configuración por entorno

Ningún secreto va en el repositorio. Los archivos `appsettings*.json` solo tienen valores de ejemplo.

### Desarrollo local
1. La clave del JWT para desarrollo ya está en `appsettings.Development.json` (no es un secreto).
2. Levanta PostgreSQL local con `docker compose up -d postgres` (usuario `postgres`, clave de desarrollo ya incluida en `appsettings.Development.json`, base `routeguard`). Si usas tu propio PostgreSQL, guarda la cadena de conexión en *user-secrets* (queda en tu usuario de Windows, fuera del repo):
   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=routeguard;Username=postgres;Password=TU_CLAVE" --project RouteGuard.Platform
   ```
3. `dotnet run` con el perfil `http` (puerto 8080). Se crean la base y los datos de demostración.

### Producción
Variables de entorno (el backend se niega a arrancar si la clave del JWT falta o tiene menos de 32 caracteres):

| Variable | Para qué |
|---|---|
| `JWT_SECRET` | Clave de firma del JWT (mínimo 32 caracteres) |
| `DATABASE_URL`, `DATABASE_PORT`, `DATABASE_SCHEMA`, `DATABASE_USER`, `DATABASE_PASSWORD` | Conexión a PostgreSQL |
| `RABBITMQ_URL` | Conexión a RabbitMQ |
| `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, … | Orígenes web permitidos. Sin ninguno, no se permite CORS (las apps móviles no lo necesitan) |
| `HardwareAdapter__ApiKey` | Clave que debe enviar el hardware en `X-Api-Key`. Sin ella el adaptador responde 503 fuera de desarrollo |
| `Seed__Enabled` | `true` solo si quieres los datos de demostración (usuarios con claves conocidas). Por defecto es `false` en producción |

Los planes y las plantillas de notificación se crean siempre; los usuarios, rutas y viajes de demostración, solo en desarrollo.

## 3. Pendiente

- Acotar los datos de los ADMIN por organización (hoy un ADMIN ve todas).
- Acotar las notificaciones que ve un DRIVER a las de sus viajes.
- Refresh de token, recuperación de contraseña, política de contraseñas y límite de intentos.
- **Rotar credenciales expuestas:** la contraseña de la base de datos (MySQL) y la clave del JWT que estuvieron en `appsettings.json` / `appsettings.Production.json` siguen en el historial de Git. Hay que cambiarlas en los servidores donde se usaron.
