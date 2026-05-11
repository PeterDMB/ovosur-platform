using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using OVOSUR.Api.Modules.Security.Contracts;
using OVOSUR.Api.Modules.Security.Entities;
using OVOSUR.Api.Modules.Suppliers.Contracts;
using OVOSUR.Api.Modules.Suppliers.Services;

namespace OVOSUR.Api.Modules.Suppliers;

public static class SupplierEndpoints
{
    public static IEndpointRouteBuilder MapSupplierEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/suppliers")
            .WithTags("Suppliers")
            .RequireAuthorization(policy => policy.RequireRole(RoleCodes.SuperAdmin));

        group.MapGet("/", async (
            [FromQuery] string? status,
            SupplierService supplierService,
            CancellationToken cancellationToken) =>
        {
            var result = await supplierService.ListAsync(status, cancellationToken);
            return result.ToHttpResult();
        });

        group.MapPatch("/{proveedorId:int}/approval", async (
            int proveedorId,
            [FromBody] UpdateSupplierApprovalRequest request,
            ClaimsPrincipal principal,
            SupplierService supplierService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var adminUserId))
            {
                return Results.Unauthorized();
            }

            var result = await supplierService.UpdateApprovalAsync(
                proveedorId,
                request,
                adminUserId,
                httpContext,
                cancellationToken);
            return result.ToHttpResult();
        });

        return endpoints;
    }
}
