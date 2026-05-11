namespace OVOSUR.Api.Modules.Security.Entities;

public sealed class RolPermiso
{
    public int RolId { get; set; }
    public int SubmoduloId { get; set; }
    public int PermisoId { get; set; }

    public Rol Rol { get; set; } = null!;
    public Submodulo Submodulo { get; set; } = null!;
    public Permiso Permiso { get; set; } = null!;
}
