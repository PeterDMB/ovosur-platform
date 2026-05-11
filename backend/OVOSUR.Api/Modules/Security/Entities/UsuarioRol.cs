namespace OVOSUR.Api.Modules.Security.Entities;

public sealed class UsuarioRol
{
    public Guid UsuarioId { get; set; }
    public int RolId { get; set; }
    public DateTime FechaAsignacion { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public Rol Rol { get; set; } = null!;
}
