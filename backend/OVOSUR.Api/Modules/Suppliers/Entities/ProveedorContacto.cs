namespace OVOSUR.Api.Modules.Suppliers.Entities;

public sealed class ProveedorContacto
{
    public int ProveedorContactoId { get; set; }
    public int ProveedorId { get; set; }
    public string TipoContacto { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string? Apellidos { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public bool EsPrincipal { get; set; }
    public bool Activo { get; set; } = true;

    public Proveedor Proveedor { get; set; } = null!;
}
