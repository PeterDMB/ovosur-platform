namespace OVOSUR.Api.Modules.Suppliers.Contracts;

public sealed record SupplierApprovalResponse(
    int ProveedorId,
    Guid UsuarioId,
    string Ruc,
    string RazonSocial,
    string Email,
    string EstadoAprobacion,
    string EstadoHomologacion,
    DateTime FechaRegistro);

public sealed record UpdateSupplierApprovalRequest(string Decision, string? Reason);

public sealed record UpdateSupplierApprovalResponse(
    int ProveedorId,
    string Ruc,
    string RazonSocial,
    string EstadoAprobacion,
    string EstadoHomologacion);
