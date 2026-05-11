namespace OVOSUR.Api.Modules.Security.Entities;

public sealed class Rol
{
    public int RolId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool EsSistema { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; }

    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = [];
    public ICollection<RolPermiso> RolPermisos { get; set; } = [];
}
