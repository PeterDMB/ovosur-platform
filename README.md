# OVOSUR Platform

Plataforma corporativa para intranet, extranet de proveedores e integracion con ERP Genesys.

## Stack inicial

- Backend: ASP.NET Core Web API (.NET 9)
- Frontend web: React + Vite + TypeScript
- Base de datos: SQL Server
- Mobile: React Native / Expo, pendiente de activar cuando la API este estable
- Repositorio remoto: `https://github.com/PeterDMB/ovosur-platform.git`

## Estructura

```txt
OVOSUR.Platform
|-- backend/OVOSUR.Api
|-- frontend/ovosur-web
|-- mobile
|-- database
|   |-- scripts
|   |-- migrations
|   `-- seed
`-- docs
```

## Comandos locales

Backend:

```powershell
dotnet run --project backend\OVOSUR.Api\OVOSUR.Api.csproj
```

Frontend:

```powershell
cd frontend\ovosur-web
npm run dev
```

## Autenticacion local

La API lee credenciales locales desde `backend/OVOSUR.Api/appsettings.Local.json`. Ese archivo no se sube a GitHub.

Para configurar o cambiar el Super Admin en desarrollo:

```powershell
Invoke-RestMethod `
  -Uri "http://localhost:5274/api/auth/dev/bootstrap-super-admin" `
  -Method Post `
  -ContentType "application/json" `
  -Body '{"email":"admin@ovosur.local","password":"TU_PASSWORD_ADMIN"}'
```

Endpoints iniciales:

```txt
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/register-provider
GET  /api/auth/me
POST /api/auth/dev/bootstrap-super-admin
```

## Flujo Git recomendado

```powershell
git status
git add .
git commit -m "Crear estructura inicial de OVOSUR Platform"
git push -u origin main
```

Para nuevas funcionalidades se trabajara con ramas:

```txt
feature/auth
feature/proveedores
feature/ventas
feature/erp-integracion
```
