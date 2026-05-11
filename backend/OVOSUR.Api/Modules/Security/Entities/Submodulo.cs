namespace OVOSUR.Api.Modules.Security.Entities;

public sealed class Submodulo
{
    public int SubmoduloId { get; set; }
    public int ModuloId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Ruta { get; set; }
    public int Orden { get; set; }
    public bool Activo { get; set; } = true;

    public Modulo Modulo { get; set; } = null!;
    public ICollection<RolPermiso> RolPermisos { get; set; } = [];
}
