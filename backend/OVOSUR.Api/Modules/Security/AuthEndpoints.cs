using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using OVOSUR.Api.Modules.Security.Contracts;
using OVOSUR.Api.Modules.Security.Services;

namespace OVOSUR.Api.Modules.Security;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            AuthService authService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await authService.LoginAsync(request, httpContext, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapPost("/refresh", async (
            [FromBody] RefreshTokenRequest request,
            AuthService authService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await authService.RefreshAsync(request, httpContext, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapPost("/register-provider", async (
            [FromBody] RegisterProviderRequest request,
            AuthService authService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await authService.RegisterProviderAsync(request, httpContext, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapGet("/me", async (
            ClaimsPrincipal principal,
            AuthService authService,
            CancellationToken cancellationToken) =>
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var usuarioId))
            {
                return Results.Unauthorized();
            }

            var result = await authService.GetCurrentUserAsync(usuarioId, cancellationToken);
            return result.ToHttpResult();
        }).RequireAuthorization();

        group.MapPost("/dev/bootstrap-super-admin", async (
            [FromBody] BootstrapSuperAdminRequest request,
            AuthService authService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await authService.BootstrapSuperAdminAsync(request, httpContext, cancellationToken);
            return result.ToHttpResult();
        });

        return endpoints;
    }
}
