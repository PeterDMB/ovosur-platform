namespace OVOSUR.Api.Modules.Security.Contracts;

public sealed record LoginRequest(string Email, string Password);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record RegisterProviderRequest(
    string Ruc,
    string RazonSocial,
    string Email,
    string Password,
    string? NombreComercial,
    string? DireccionFiscal,
    string? Telefono);

public sealed record BootstrapSuperAdminRequest(string Email, string Password);

public sealed record AuthSessionResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    AuthUserResponse User);

public sealed record AuthUserResponse(
    Guid UsuarioId,
    string Email,
    string TipoUsuario,
    string EstadoAprobacion,
    IReadOnlyCollection<string> Roles,
    SupplierSummaryResponse? Proveedor);

public sealed record SupplierSummaryResponse(int ProveedorId, string Ruc, string RazonSocial, string EstadoHomologacion);

public sealed record RegisterProviderResponse(Guid UsuarioId, int ProveedorId, string EstadoAprobacion, string EstadoHomologacion);

public sealed record BootstrapSuperAdminResponse(Guid UsuarioId, string Email, string Estado);

public sealed record ApiErrorResponse(string Code, string Message);

public sealed record ServiceResult<T>(bool Succeeded, int StatusCode, T? Value, string? Code, string? Message)
{
    public static ServiceResult<T> Ok(T value) => new(true, StatusCodes.Status200OK, value, null, null);
    public static ServiceResult<T> Created(T value) => new(true, StatusCodes.Status201Created, value, null, null);
    public static ServiceResult<T> Fail(int statusCode, string code, string message) => new(false, statusCode, default, code, message);
}

public static class ServiceResultExtensions
{
    public static IResult ToHttpResult<T>(this ServiceResult<T> result)
    {
        if (result.Succeeded)
        {
            return result.StatusCode == StatusCodes.Status201Created
                ? Results.Json(result.Value, statusCode: StatusCodes.Status201Created)
                : Results.Ok(result.Value);
        }

        var error = new ApiErrorResponse(result.Code ?? "ERROR", result.Message ?? "No se pudo procesar la solicitud.");
        return Results.Json(error, statusCode: result.StatusCode);
    }
}
