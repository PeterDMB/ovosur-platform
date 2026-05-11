IF DB_ID(N'OVOSUR_INTRANET') IS NULL
BEGIN
    CREATE DATABASE OVOSUR_INTRANET;
END;
GO

USE OVOSUR_INTRANET;
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Seguridad')
    EXEC(N'CREATE SCHEMA Seguridad');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Core')
    EXEC(N'CREATE SCHEMA Core');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Proveedores')
    EXEC(N'CREATE SCHEMA Proveedores');
GO

CREATE TABLE Seguridad.Modulos
(
    ModuloId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Modulos PRIMARY KEY,
    Codigo NVARCHAR(50) NOT NULL,
    Nombre NVARCHAR(120) NOT NULL,
    Descripcion NVARCHAR(300) NULL,
    Orden INT NOT NULL CONSTRAINT DF_Modulos_Orden DEFAULT (0),
    Activo BIT NOT NULL CONSTRAINT DF_Modulos_Activo DEFAULT (1),
    FechaCreacion DATETIME2(0) NOT NULL CONSTRAINT DF_Modulos_FechaCreacion DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Modulos_Codigo UNIQUE (Codigo)
);
GO

CREATE TABLE Seguridad.Submodulos
(
    SubmoduloId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Submodulos PRIMARY KEY,
    ModuloId INT NOT NULL,
    Codigo NVARCHAR(50) NOT NULL,
    Nombre NVARCHAR(120) NOT NULL,
    Ruta NVARCHAR(180) NULL,
    Orden INT NOT NULL CONSTRAINT DF_Submodulos_Orden DEFAULT (0),
    Activo BIT NOT NULL CONSTRAINT DF_Submodulos_Activo DEFAULT (1),
    CONSTRAINT FK_Submodulos_Modulos FOREIGN KEY (ModuloId) REFERENCES Seguridad.Modulos(ModuloId),
    CONSTRAINT UQ_Submodulos_Modulo_Codigo UNIQUE (ModuloId, Codigo)
);
GO

CREATE TABLE Seguridad.Permisos
(
    PermisoId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Permisos PRIMARY KEY,
    Codigo NVARCHAR(40) NOT NULL,
    Nombre NVARCHAR(80) NOT NULL,
    CONSTRAINT UQ_Permisos_Codigo UNIQUE (Codigo)
);
GO

CREATE TABLE Seguridad.Roles
(
    RolId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
    Codigo NVARCHAR(50) NOT NULL,
    Nombre NVARCHAR(120) NOT NULL,
    Descripcion NVARCHAR(300) NULL,
    EsSistema BIT NOT NULL CONSTRAINT DF_Roles_EsSistema DEFAULT (0),
    Activo BIT NOT NULL CONSTRAINT DF_Roles_Activo DEFAULT (1),
    FechaCreacion DATETIME2(0) NOT NULL CONSTRAINT DF_Roles_FechaCreacion DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Roles_Codigo UNIQUE (Codigo)
);
GO

CREATE TABLE Seguridad.Usuarios
(
    UsuarioId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Usuarios PRIMARY KEY DEFAULT NEWID(),
    Email NVARCHAR(180) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    TipoUsuario NVARCHAR(20) NOT NULL,
    EstadoAprobacion NVARCHAR(20) NOT NULL CONSTRAINT DF_Usuarios_EstadoAprobacion DEFAULT (N'PENDIENTE'),
    IntentosFallidos INT NOT NULL CONSTRAINT DF_Usuarios_IntentosFallidos DEFAULT (0),
    BloqueadoHasta DATETIME2(0) NULL,
    UltimoAcceso DATETIME2(0) NULL,
    DebeCambiarPassword BIT NOT NULL CONSTRAINT DF_Usuarios_DebeCambiarPassword DEFAULT (0),
    Activo BIT NOT NULL CONSTRAINT DF_Usuarios_Activo DEFAULT (1),
    FechaCreacion DATETIME2(0) NOT NULL CONSTRAINT DF_Usuarios_FechaCreacion DEFAULT SYSUTCDATETIME(),
    FechaActualizacion DATETIME2(0) NULL,
    CONSTRAINT UQ_Usuarios_Email UNIQUE (Email),
    CONSTRAINT CK_Usuarios_TipoUsuario CHECK (TipoUsuario IN (N'INTERNO', N'PROVEEDOR')),
    CONSTRAINT CK_Usuarios_EstadoAprobacion CHECK (EstadoAprobacion IN (N'PENDIENTE', N'APROBADO', N'RECHAZADO'))
);
GO

CREATE TABLE Seguridad.UsuarioRoles
(
    UsuarioId UNIQUEIDENTIFIER NOT NULL,
    RolId INT NOT NULL,
    FechaAsignacion DATETIME2(0) NOT NULL CONSTRAINT DF_UsuarioRoles_FechaAsignacion DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_UsuarioRoles PRIMARY KEY (UsuarioId, RolId),
    CONSTRAINT FK_UsuarioRoles_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Seguridad.Usuarios(UsuarioId),
    CONSTRAINT FK_UsuarioRoles_Roles FOREIGN KEY (RolId) REFERENCES Seguridad.Roles(RolId)
);
GO

CREATE TABLE Seguridad.RolPermisos
(
    RolId INT NOT NULL,
    SubmoduloId INT NOT NULL,
    PermisoId INT NOT NULL,
    CONSTRAINT PK_RolPermisos PRIMARY KEY (RolId, SubmoduloId, PermisoId),
    CONSTRAINT FK_RolPermisos_Roles FOREIGN KEY (RolId) REFERENCES Seguridad.Roles(RolId),
    CONSTRAINT FK_RolPermisos_Submodulos FOREIGN KEY (SubmoduloId) REFERENCES Seguridad.Submodulos(SubmoduloId),
    CONSTRAINT FK_RolPermisos_Permisos FOREIGN KEY (PermisoId) REFERENCES Seguridad.Permisos(PermisoId)
);
GO

CREATE TABLE Seguridad.RefreshTokens
(
    RefreshTokenId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RefreshTokens PRIMARY KEY,
    UsuarioId UNIQUEIDENTIFIER NOT NULL,
    TokenHash NVARCHAR(300) NOT NULL,
    FechaExpiracion DATETIME2(0) NOT NULL,
    FechaRevocacion DATETIME2(0) NULL,
    IpCreacion NVARCHAR(64) NULL,
    UserAgent NVARCHAR(300) NULL,
    CONSTRAINT FK_RefreshTokens_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Seguridad.Usuarios(UsuarioId)
);
GO

CREATE TABLE Seguridad.AuditoriaEventos
(
    AuditoriaEventoId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditoriaEventos PRIMARY KEY,
    UsuarioId UNIQUEIDENTIFIER NULL,
    Evento NVARCHAR(120) NOT NULL,
    Entidad NVARCHAR(120) NULL,
    EntidadId NVARCHAR(80) NULL,
    Ip NVARCHAR(64) NULL,
    UserAgent NVARCHAR(300) NULL,
    MetadataJson NVARCHAR(MAX) NULL,
    FechaEvento DATETIME2(0) NOT NULL CONSTRAINT DF_AuditoriaEventos_FechaEvento DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_AuditoriaEventos_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Seguridad.Usuarios(UsuarioId)
);
GO

CREATE TABLE Core.Empleados
(
    EmpleadoId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Empleados PRIMARY KEY,
    UsuarioId UNIQUEIDENTIFIER NOT NULL,
    CodigoEmpleado NVARCHAR(30) NULL,
    Nombres NVARCHAR(120) NOT NULL,
    Apellidos NVARCHAR(120) NOT NULL,
    Area NVARCHAR(80) NULL,
    Cargo NVARCHAR(100) NULL,
    Activo BIT NOT NULL CONSTRAINT DF_Empleados_Activo DEFAULT (1),
    CONSTRAINT UQ_Empleados_UsuarioId UNIQUE (UsuarioId),
    CONSTRAINT FK_Empleados_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Seguridad.Usuarios(UsuarioId)
);
GO

CREATE TABLE Proveedores.Proveedores
(
    ProveedorId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Proveedores PRIMARY KEY,
    UsuarioId UNIQUEIDENTIFIER NOT NULL,
    Ruc CHAR(11) NOT NULL,
    RazonSocial NVARCHAR(200) NOT NULL,
    NombreComercial NVARCHAR(200) NULL,
    DireccionFiscal NVARCHAR(250) NULL,
    Telefono NVARCHAR(40) NULL,
    EstadoHomologacion NVARCHAR(20) NOT NULL CONSTRAINT DF_Proveedores_EstadoHomologacion DEFAULT (N'EN_REVISION'),
    FechaRegistro DATETIME2(0) NOT NULL CONSTRAINT DF_Proveedores_FechaRegistro DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Proveedores_UsuarioId UNIQUE (UsuarioId),
    CONSTRAINT UQ_Proveedores_Ruc UNIQUE (Ruc),
    CONSTRAINT FK_Proveedores_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Seguridad.Usuarios(UsuarioId),
    CONSTRAINT CK_Proveedores_EstadoHomologacion CHECK (EstadoHomologacion IN (N'EN_REVISION', N'APROBADO', N'OBSERVADO', N'RECHAZADO'))
);
GO

CREATE TABLE Proveedores.ProveedorContactos
(
    ProveedorContactoId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProveedorContactos PRIMARY KEY,
    ProveedorId INT NOT NULL,
    TipoContacto NVARCHAR(30) NOT NULL,
    Nombres NVARCHAR(120) NOT NULL,
    Apellidos NVARCHAR(120) NULL,
    Email NVARCHAR(180) NULL,
    Telefono NVARCHAR(40) NULL,
    EsPrincipal BIT NOT NULL CONSTRAINT DF_ProveedorContactos_EsPrincipal DEFAULT (0),
    Activo BIT NOT NULL CONSTRAINT DF_ProveedorContactos_Activo DEFAULT (1),
    CONSTRAINT FK_ProveedorContactos_Proveedores FOREIGN KEY (ProveedorId) REFERENCES Proveedores.Proveedores(ProveedorId),
    CONSTRAINT CK_ProveedorContactos_TipoContacto CHECK (TipoContacto IN (N'GERENCIAL', N'COMERCIAL', N'FACTURACION', N'OPERACIONES'))
);
GO

CREATE TABLE Proveedores.ProveedorDocumentos
(
    ProveedorDocumentoId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProveedorDocumentos PRIMARY KEY,
    ProveedorId INT NOT NULL,
    TipoDocumento NVARCHAR(60) NOT NULL,
    NombreArchivoOriginal NVARCHAR(260) NOT NULL,
    RutaFisica NVARCHAR(500) NOT NULL,
    MimeType NVARCHAR(120) NOT NULL,
    TamanoBytes BIGINT NOT NULL,
    EstadoRevision NVARCHAR(20) NOT NULL CONSTRAINT DF_ProveedorDocumentos_EstadoRevision DEFAULT (N'PENDIENTE'),
    FechaCarga DATETIME2(0) NOT NULL CONSTRAINT DF_ProveedorDocumentos_FechaCarga DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ProveedorDocumentos_Proveedores FOREIGN KEY (ProveedorId) REFERENCES Proveedores.Proveedores(ProveedorId),
    CONSTRAINT CK_ProveedorDocumentos_EstadoRevision CHECK (EstadoRevision IN (N'PENDIENTE', N'APROBADO', N'OBSERVADO', N'RECHAZADO'))
);
GO

INSERT INTO Seguridad.Permisos (Codigo, Nombre)
SELECT Codigo, Nombre
FROM (VALUES
    (N'VER', N'Ver'),
    (N'CREAR', N'Crear'),
    (N'EDITAR', N'Editar'),
    (N'APROBAR', N'Aprobar'),
    (N'RECHAZAR', N'Rechazar'),
    (N'EXPORTAR', N'Exportar'),
    (N'ELIMINAR', N'Eliminar')
) AS P(Codigo, Nombre)
WHERE NOT EXISTS (SELECT 1 FROM Seguridad.Permisos X WHERE X.Codigo = P.Codigo);
GO

INSERT INTO Seguridad.Roles (Codigo, Nombre, Descripcion, EsSistema)
SELECT N'SUPER_ADMIN', N'Super Admin', N'Control total de la plataforma OVOSUR', 1
WHERE NOT EXISTS (SELECT 1 FROM Seguridad.Roles WHERE Codigo = N'SUPER_ADMIN');
GO

INSERT INTO Seguridad.Modulos (Codigo, Nombre, Descripcion, Orden)
SELECT Codigo, Nombre, Descripcion, Orden
FROM (VALUES
    (N'SEGURIDAD', N'Seguridad', N'Usuarios, roles y permisos', 1),
    (N'PROVEEDORES', N'Proveedores', N'Portal y homologacion de proveedores', 2),
    (N'VENTAS', N'Ventas', N'Pedidos, clientes y stock comercial', 3),
    (N'TESORERIA', N'Tesoreria', N'Aprobaciones financieras', 4),
    (N'DISTRIBUCION', N'Distribucion', N'Horarios y despacho', 5)
) AS M(Codigo, Nombre, Descripcion, Orden)
WHERE NOT EXISTS (SELECT 1 FROM Seguridad.Modulos X WHERE X.Codigo = M.Codigo);
GO

DECLARE @SuperAdminId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
DECLARE @SuperAdminRoleId INT = (SELECT RolId FROM Seguridad.Roles WHERE Codigo = N'SUPER_ADMIN');

IF NOT EXISTS (SELECT 1 FROM Seguridad.Usuarios WHERE UsuarioId = @SuperAdminId)
BEGIN
    INSERT INTO Seguridad.Usuarios
    (
        UsuarioId,
        Email,
        PasswordHash,
        TipoUsuario,
        EstadoAprobacion,
        DebeCambiarPassword,
        Activo
    )
    VALUES
    (
        @SuperAdminId,
        N'admin@ovosur.local',
        N'REEMPLAZAR_CON_HASH_GENERADO_POR_LA_API',
        N'INTERNO',
        N'APROBADO',
        1,
        1
    );
END;

IF NOT EXISTS (SELECT 1 FROM Seguridad.UsuarioRoles WHERE UsuarioId = @SuperAdminId AND RolId = @SuperAdminRoleId)
BEGIN
    INSERT INTO Seguridad.UsuarioRoles (UsuarioId, RolId)
    VALUES (@SuperAdminId, @SuperAdminRoleId);
END;
GO
