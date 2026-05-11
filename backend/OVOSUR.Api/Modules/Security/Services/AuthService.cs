using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OVOSUR.Api.Infrastructure.Persistence;
using OVOSUR.Api.Modules.Security.Contracts;
using OVOSUR.Api.Modules.Security.Entities;
using OVOSUR.Api.Modules.Suppliers.Entities;

namespace OVOSUR.Api.Modules.Security.Services;

public sealed class AuthService(
    OvosurDbContext dbContext,
    PasswordHasher<Usuario> passwordHasher,
    TokenService tokenService,
    IWebHostEnvironment environment)
{
    private static readonly TimeSpan FailedLoginLockDuration = TimeSpan.FromMinutes(15);

    public async Task<ServiceResult<AuthSessionResponse>> LoginAsync(
        LoginRequest request,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var now = DateTime.UtcNow;
        var usuario = await dbContext.Usuarios
            .Include(x => x.UsuarioRoles)
                .ThenInclude(x => x.Rol)
            .Include(x => x.Proveedor)
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

        if (usuario is null)
        {
            return ServiceResult<AuthSessionResponse>.Fail(
                StatusCodes.Status401Unauthorized,
                "INVALID_CREDENTIALS",
                "Correo o contrasena incorrectos.");
        }

        if (!usuario.Activo)
        {
            return ServiceResult<AuthSessionResponse>.Fail(
                StatusCodes.Status403Forbidden,
                "USER_INACTIVE",
                "La cuenta se encuentra inactiva.");
        }

        if (usuario.BloqueadoHasta is not null && usuario.BloqueadoHasta > now)
        {
            return ServiceResult<AuthSessionResponse>.Fail(
                StatusCodes.Status403Forbidden,
                "USER_LOCKED",
                $"La cuenta esta bloqueada hasta {usuario.BloqueadoHasta:yyyy-MM-dd HH:mm:ss} UTC.");
        }

        if (usuario.EstadoAprobacion != ApprovalStates.Aprobado)
        {
            return ServiceResult<AuthSessionResponse>.Fail(
                StatusCodes.Status403Forbidden,
                "USER_NOT_APPROVED",
                "La cuenta aun no esta aprobada para ingresar.");
        }

        var passwordResult = passwordHasher.VerifyHashedPassword(usuario, usuario.PasswordHash, request.Password);
        if (passwordResult == PasswordVerificationResult.Failed)
        {
            usuario.IntentosFallidos += 1;

            if (usuario.IntentosFallidos >= 3)
            {
                usuario.BloqueadoHasta = now.Add(FailedLoginLockDuration);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return ServiceResult<AuthSessionResponse>.Fail(
                StatusCodes.Status401Unauthorized,
                "INVALID_CREDENTIALS",
                usuario.IntentosFallidos >= 3
                    ? "Cuenta bloqueada por 15 minutos tras varios intentos fallidos."
                    : "Correo o contrasena incorrectos.");
        }

        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHasta = null;
        usuario.UltimoAcceso = now;
        usuario.FechaActualizacion = now;

        var response = CreateSession(usuario, httpContext);
        AddAudit(usuario.UsuarioId, "AUTH_LOGIN_SUCCESS", "Seguridad.Usuarios", usuario.UsuarioId.ToString(), httpContext);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<AuthSessionResponse>.Ok(response);
    }

    public async Task<ServiceResult<AuthSessionResponse>> RefreshAsync(
        RefreshTokenRequest request,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var tokenHash = TokenService.HashRefreshToken(request.RefreshToken);
        var now = DateTime.UtcNow;
        var storedToken = await dbContext.RefreshTokens
            .Include(x => x.Usuario)
                .ThenInclude(x => x.UsuarioRoles)
                    .ThenInclude(x => x.Rol)
            .Include(x => x.Usuario)
                .ThenInclude(x => x.Proveedor)
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash &&
                     x.FechaRevocacion == null &&
                     x.FechaExpiracion > now,
                cancellationToken);

        if (storedToken is null || !storedToken.Usuario.Activo || storedToken.Usuario.EstadoAprobacion != ApprovalStates.Aprobado)
        {
            return ServiceResult<AuthSessionResponse>.Fail(
                StatusCodes.Status401Unauthorized,
                "INVALID_REFRESH_TOKEN",
                "La sesion ya no es valida. Inicia sesion nuevamente.");
        }

        storedToken.FechaRevocacion = now;
        var response = CreateSession(storedToken.Usuario, httpContext);
        AddAudit(storedToken.UsuarioId, "AUTH_REFRESH_TOKEN", "Seguridad.RefreshTokens", storedToken.RefreshTokenId.ToString(), httpContext);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<AuthSessionResponse>.Ok(response);
    }

    public async Task<ServiceResult<RegisterProviderResponse>> RegisterProviderAsync(
        RegisterProviderRequest request,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var ruc = request.Ruc.Trim();

        if (ruc.Length != 11 || !ruc.All(char.IsDigit))
        {
            return ServiceResult<RegisterProviderResponse>.Fail(
                StatusCodes.Status400BadRequest,
                "INVALID_RUC",
                "El RUC debe tener 11 digitos.");
        }

        if (request.Password.Length < 8)
        {
            return ServiceResult<RegisterProviderResponse>.Fail(
                StatusCodes.Status400BadRequest,
                "WEAK_PASSWORD",
                "La contrasena debe tener al menos 8 caracteres.");
        }

        var exists = await dbContext.Usuarios.AnyAsync(x => x.Email == email, cancellationToken);
        if (exists)
        {
            return ServiceResult<RegisterProviderResponse>.Fail(
                StatusCodes.Status409Conflict,
                "EMAIL_EXISTS",
                "Ya existe un usuario con ese correo.");
        }

        var rucExists = await dbContext.Proveedores.AnyAsync(x => x.Ruc == ruc, cancellationToken);
        if (rucExists)
        {
            return ServiceResult<RegisterProviderResponse>.Fail(
                StatusCodes.Status409Conflict,
                "RUC_EXISTS",
                "Ya existe un proveedor registrado con ese RUC.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var usuario = new Usuario
        {
            UsuarioId = Guid.NewGuid(),
            Email = email,
            TipoUsuario = UserTypes.Proveedor,
            EstadoAprobacion = ApprovalStates.Pendiente,
            Activo = true,
            FechaCreacion = now
        };
        usuario.PasswordHash = passwordHasher.HashPassword(usuario, request.Password);

        var proveedor = new Proveedor
        {
            UsuarioId = usuario.UsuarioId,
            Ruc = ruc,
            RazonSocial = request.RazonSocial.Trim(),
            NombreComercial = request.NombreComercial?.Trim(),
            DireccionFiscal = request.DireccionFiscal?.Trim(),
            Telefono = request.Telefono?.Trim(),
            EstadoHomologacion = SupplierStates.EnRevision,
            FechaRegistro = now
        };

        dbContext.Usuarios.Add(usuario);
        dbContext.Proveedores.Add(proveedor);
        await dbContext.SaveChangesAsync(cancellationToken);

        await EnsureRoleAssignmentAsync(usuario, RoleCodes.Proveedor, "Proveedor", cancellationToken);
        AddAudit(usuario.UsuarioId, "SUPPLIER_REGISTERED", "Proveedores.Proveedores", proveedor.ProveedorId.ToString(), httpContext);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ServiceResult<RegisterProviderResponse>.Created(new RegisterProviderResponse(
            usuario.UsuarioId,
            proveedor.ProveedorId,
            usuario.EstadoAprobacion,
            proveedor.EstadoHomologacion));
    }

    public async Task<ServiceResult<AuthUserResponse>> GetCurrentUserAsync(
        Guid usuarioId,
        CancellationToken cancellationToken)
    {
        var usuario = await dbContext.Usuarios
            .Include(x => x.UsuarioRoles)
                .ThenInclude(x => x.Rol)
            .Include(x => x.Proveedor)
            .FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);

        if (usuario is null)
        {
            return ServiceResult<AuthUserResponse>.Fail(
                StatusCodes.Status404NotFound,
                "USER_NOT_FOUND",
                "Usuario no encontrado.");
        }

        return ServiceResult<AuthUserResponse>.Ok(MapUser(usuario));
    }

    public async Task<ServiceResult<BootstrapSuperAdminResponse>> BootstrapSuperAdminAsync(
        BootstrapSuperAdminRequest request,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return ServiceResult<BootstrapSuperAdminResponse>.Fail(
                StatusCodes.Status404NotFound,
                "NOT_FOUND",
                "Recurso no encontrado.");
        }

        if (request.Password.Length < 10)
        {
            return ServiceResult<BootstrapSuperAdminResponse>.Fail(
                StatusCodes.Status400BadRequest,
                "WEAK_PASSWORD",
                "La contrasena del administrador debe tener al menos 10 caracteres.");
        }

        var email = request.Email.Trim();
        var now = DateTime.UtcNow;
        var usuario = await dbContext.Usuarios
            .Include(x => x.UsuarioRoles)
                .ThenInclude(x => x.Rol)
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

        if (usuario is null)
        {
            usuario = new Usuario
            {
                UsuarioId = Guid.NewGuid(),
                Email = email,
                TipoUsuario = UserTypes.Interno,
                EstadoAprobacion = ApprovalStates.Aprobado,
                Activo = true,
                FechaCreacion = now
            };
            dbContext.Usuarios.Add(usuario);
        }

        usuario.PasswordHash = passwordHasher.HashPassword(usuario, request.Password);
        usuario.TipoUsuario = UserTypes.Interno;
        usuario.EstadoAprobacion = ApprovalStates.Aprobado;
        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHasta = null;
        usuario.DebeCambiarPassword = false;
        usuario.Activo = true;
        usuario.FechaActualizacion = now;

        await dbContext.SaveChangesAsync(cancellationToken);
        await EnsureRoleAssignmentAsync(usuario, RoleCodes.SuperAdmin, "Super Admin", cancellationToken);
        AddAudit(usuario.UsuarioId, "DEV_BOOTSTRAP_SUPER_ADMIN", "Seguridad.Usuarios", usuario.UsuarioId.ToString(), httpContext);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<BootstrapSuperAdminResponse>.Ok(new BootstrapSuperAdminResponse(
            usuario.UsuarioId,
            usuario.Email,
            "SUPER_ADMIN_CONFIGURADO"));
    }

    private AuthSessionResponse CreateSession(
        Usuario usuario,
        HttpContext httpContext)
    {
        var roles = usuario.UsuarioRoles
            .Where(x => x.Rol.Activo)
            .Select(x => x.Rol.Codigo)
            .OrderBy(x => x)
            .ToArray();

        var accessToken = tokenService.CreateAccessToken(usuario, roles);
        var refreshToken = tokenService.CreateRefreshToken();
        dbContext.RefreshTokens.Add(new RefreshToken
        {
            UsuarioId = usuario.UsuarioId,
            TokenHash = TokenService.HashRefreshToken(refreshToken),
            FechaExpiracion = DateTime.UtcNow.AddDays(7),
            IpCreacion = GetIp(httpContext),
            UserAgent = GetUserAgent(httpContext)
        });

        return new AuthSessionResponse(accessToken.Token, refreshToken, accessToken.ExpiresAt, MapUser(usuario));
    }

    private async Task EnsureRoleAssignmentAsync(
        Usuario usuario,
        string roleCode,
        string roleName,
        CancellationToken cancellationToken)
    {
        var role = await dbContext.Roles.FirstOrDefaultAsync(x => x.Codigo == roleCode, cancellationToken);
        if (role is null)
        {
            role = new Rol
            {
                Codigo = roleCode,
                Nombre = roleName,
                Activo = true,
                EsSistema = true,
                FechaCreacion = DateTime.UtcNow
            };
            dbContext.Roles.Add(role);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var alreadyAssigned = await dbContext.UsuarioRoles
            .AnyAsync(x => x.UsuarioId == usuario.UsuarioId && x.RolId == role.RolId, cancellationToken);

        if (!alreadyAssigned)
        {
            dbContext.UsuarioRoles.Add(new UsuarioRol
            {
                UsuarioId = usuario.UsuarioId,
                RolId = role.RolId,
                FechaAsignacion = DateTime.UtcNow
            });
        }
    }

    private void AddAudit(
        Guid? usuarioId,
        string evento,
        string? entidad,
        string? entidadId,
        HttpContext httpContext)
    {
        dbContext.AuditoriaEventos.Add(new AuditoriaEvento
        {
            UsuarioId = usuarioId,
            Evento = evento,
            Entidad = entidad,
            EntidadId = entidadId,
            Ip = GetIp(httpContext),
            UserAgent = GetUserAgent(httpContext),
            FechaEvento = DateTime.UtcNow
        });
    }

    private static AuthUserResponse MapUser(Usuario usuario)
    {
        var roles = usuario.UsuarioRoles
            .Where(x => x.Rol.Activo)
            .Select(x => x.Rol.Codigo)
            .OrderBy(x => x)
            .ToArray();

        var supplier = usuario.Proveedor is null
            ? null
            : new SupplierSummaryResponse(
                usuario.Proveedor.ProveedorId,
                usuario.Proveedor.Ruc,
                usuario.Proveedor.RazonSocial,
                usuario.Proveedor.EstadoHomologacion);

        return new AuthUserResponse(
            usuario.UsuarioId,
            usuario.Email,
            usuario.TipoUsuario,
            usuario.EstadoAprobacion,
            roles,
            supplier);
    }

    private static string? GetIp(HttpContext httpContext)
    {
        return httpContext.Connection.RemoteIpAddress?.ToString();
    }

    private static string? GetUserAgent(HttpContext httpContext)
    {
        return httpContext.Request.Headers.UserAgent.ToString();
    }
}
