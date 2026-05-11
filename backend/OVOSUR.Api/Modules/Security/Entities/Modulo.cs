namespace OVOSUR.Api.Modules.Security.Entities;

public sealed class Modulo
{
    public int ModuloId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Orden { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; }

    public ICollection<Submodulo> Submodulos { get; set; } = [];
}
