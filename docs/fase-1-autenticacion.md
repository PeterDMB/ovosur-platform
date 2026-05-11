# Fase 1 - Autenticacion

## Alcance implementado

- Entidades EF Core para seguridad, empleados y proveedores.
- Mapeo a los esquemas SQL Server `Seguridad`, `Core` y `Proveedores`.
- Login con hash de contrasena usando `PasswordHasher` de ASP.NET Core.
- Bloqueo temporal tras 3 intentos fallidos.
- JWT de acceso.
- Refresh tokens persistidos con hash SHA-256.
- Registro de proveedor con estado `PENDIENTE` y homologacion `EN_REVISION`.
- Endpoint `/api/auth/me` protegido por JWT.
- Endpoint de desarrollo para inicializar el Super Admin.
- Endpoints administrativos para listar y aprobar proveedores.
- Dashboard React segun tipo de sesion.

## Endpoints

```txt
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/register-provider
GET  /api/auth/me
POST /api/auth/dev/bootstrap-super-admin
GET  /api/suppliers?status=PENDIENTE
PATCH /api/suppliers/{proveedorId}/approval
```

## Decisiones de aprobacion

El endpoint `PATCH /api/suppliers/{proveedorId}/approval` acepta:

```txt
APROBAR
OBSERVAR
RECHAZAR
```

Solo usuarios con rol `SUPER_ADMIN` pueden usar estos endpoints.

## Notas de seguridad

- `appsettings.Local.json` queda fuera de Git para evitar credenciales en GitHub.
- El endpoint de bootstrap responde solo en ambiente `Development`.
- El frontend guarda la sesion en `localStorage` temporalmente; en una fase posterior se puede endurecer con cookies `HttpOnly` si el portal lo requiere.
- Las aprobaciones de proveedores quedan pendientes de implementar en un modulo administrativo.
