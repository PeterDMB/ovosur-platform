namespace OVOSUR.Api.Modules.Security.Entities;

public sealed class Permiso
{
    public int PermisoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;

    public ICollection<RolPermiso> RolPermisos { get; set; } = [];
}
