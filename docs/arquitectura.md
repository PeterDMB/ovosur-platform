# Arquitectura OVOSUR Platform

## Decision tecnica

La plataforma se inicia con ASP.NET Core, React y SQL Server porque el entorno objetivo es una intranet corporativa sobre Windows Server, con crecimiento modular e integracion con un ERP existente.

## Modulos principales

- Seguridad y autenticacion
- Portal de proveedores
- Intranet interna
- Ventas
- Tesoreria
- Distribucion
- Integracion ERP Genesys
- Gestion documental
- Auditoria

## Separacion de identidad

La tabla `Seguridad.Usuarios` representa la identidad de acceso. Los datos de negocio se separan por contexto:

- `Core.Empleados` para personal interno.
- `Proveedores.Proveedores` para empresas proveedoras.
- `Proveedores.ProveedorContactos` para contactos gerenciales, comerciales y de facturacion.

Esta separacion evita mezclar credenciales con datos comerciales y permite que el modelo crezca sin perder seguridad.

## Seguridad base

- JWT de corta duracion para acceso.
- Refresh tokens persistidos y revocables.
- Bloqueo temporal por intentos fallidos.
- Estados de aprobacion para proveedores.
- Roles y permisos por modulo/submodulo.
- Auditoria de eventos sensibles.
- Documentos de proveedor con metadata en SQL Server y archivo fisico fuera de la base.

## Proxima fase

Implementar el modelo EF Core, endpoints de autenticacion y primera pantalla de login conectada a la API.
