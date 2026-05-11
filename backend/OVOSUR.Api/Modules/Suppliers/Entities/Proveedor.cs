using OVOSUR.Api.Modules.Security.Entities;

namespace OVOSUR.Api.Modules.Suppliers.Entities;

public sealed class Proveedor
{
    public int ProveedorId { get; set; }
    public Guid UsuarioId { get; set; }
    public string Ruc { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public string? DireccionFiscal { get; set; }
    public string? Telefono { get; set; }
    public string EstadoHomologacion { get; set; } = SupplierStates.EnRevision;
    public DateTime FechaRegistro { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public ICollection<ProveedorContacto> Contactos { get; set; } = [];
    public ICollection<ProveedorDocumento> Documentos { get; set; } = [];
}
