using Microsoft.EntityFrameworkCore;
using OVOSUR.Api.Infrastructure.Persistence;
using OVOSUR.Api.Modules.Security.Contracts;
using OVOSUR.Api.Modules.Security.Entities;
using OVOSUR.Api.Modules.Suppliers.Contracts;
using OVOSUR.Api.Modules.Suppliers.Entities;

namespace OVOSUR.Api.Modules.Suppliers.Services;

public sealed class SupplierService(OvosurDbContext dbContext)
{
    public async Task<ServiceResult<IReadOnlyCollection<SupplierApprovalResponse>>> ListAsync(
        string? status,
        CancellationToken cancellationToken)
    {
        var normalizedStatus = status?.Trim().ToUpperInvariant();
        var query = dbContext.Proveedores
            .Include(x => x.Usuario)
            .AsNoTracking()
            .AsQueryable();

        query = normalizedStatus switch
        {
            "PENDIENTE" => query.Where(x => x.Usuario.EstadoAprobacion == ApprovalStates.Pendiente),
            "APROBADO" => query.Where(x => x.Usuario.EstadoAprobacion == ApprovalStates.Aprobado),
            "RECHAZADO" => query.Where(x => x.Usuario.EstadoAprobacion == ApprovalStates.Rechazado),
            "OBSERVADO" => query.Where(x => x.EstadoHomologacion == SupplierStates.Observado),
            _ => query
        };

        var suppliers = await query
            .OrderByDescending(x => x.FechaRegistro)
            .Select(x => new SupplierApprovalResponse(
                x.ProveedorId,
                x.UsuarioId,
                x.Ruc,
                x.RazonSocial,
                x.Usuario.Email,
                x.Usuario.EstadoAprobacion,
                x.EstadoHomologacion,
                x.FechaRegistro))
            .ToArrayAsync(cancellationToken);

        return ServiceResult<IReadOnlyCollection<SupplierApprovalResponse>>.Ok(suppliers);
    }

    public async Task<ServiceResult<UpdateSupplierApprovalResponse>> UpdateApprovalAsync(
        int proveedorId,
        UpdateSupplierApprovalRequest request,
        Guid adminUserId,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var supplier = await dbContext.Proveedores
            .Include(x => x.Usuario)
            .FirstOrDefaultAsync(x => x.ProveedorId == proveedorId, cancellationToken);

        if (supplier is null)
        {
            return ServiceResult<UpdateSupplierApprovalResponse>.Fail(
                StatusCodes.Status404NotFound,
                "SUPPLIER_NOT_FOUND",
                "Proveedor no encontrado.");
        }

        var decision = request.Decision.Trim().ToUpperInvariant();
        switch (decision)
        {
            case "APROBAR":
                supplier.Usuario.EstadoAprobacion = ApprovalStates.Aprobado;
                supplier.Usuario.Activo = true;
                supplier.EstadoHomologacion = SupplierStates.Aprobado;
                break;
            case "RECHAZAR":
                supplier.Usuario.EstadoAprobacion = ApprovalStates.Rechazado;
                supplier.Usuario.Activo = false;
                supplier.EstadoHomologacion = SupplierStates.Rechazado;
                break;
            case "OBSERVAR":
                supplier.Usuario.EstadoAprobacion = ApprovalStates.Pendiente;
                supplier.Usuario.Activo = true;
                supplier.EstadoHomologacion = SupplierStates.Observado;
                break;
            default:
                return ServiceResult<UpdateSupplierApprovalResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "INVALID_DECISION",
                    "La decision debe ser APROBAR, RECHAZAR u OBSERVAR.");
        }

        supplier.Usuario.FechaActualizacion = DateTime.UtcNow;
        dbContext.AuditoriaEventos.Add(new AuditoriaEvento
        {
            UsuarioId = adminUserId,
            Evento = $"SUPPLIER_{decision}",
            Entidad = "Proveedores.Proveedores",
            EntidadId = supplier.ProveedorId.ToString(),
            Ip = httpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext.Request.Headers.UserAgent.ToString(),
            MetadataJson = string.IsNullOrWhiteSpace(request.Reason)
                ? null
                : $$"""{"reason":"{{EscapeJson(request.Reason)}}"}""",
            FechaEvento = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return ServiceResult<UpdateSupplierApprovalResponse>.Ok(new UpdateSupplierApprovalResponse(
            supplier.ProveedorId,
            supplier.Ruc,
            supplier.RazonSocial,
            supplier.Usuario.EstadoAprobacion,
            supplier.EstadoHomologacion));
    }

    private static string EscapeJson(string value)
    {
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
