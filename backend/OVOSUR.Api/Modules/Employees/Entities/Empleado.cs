using OVOSUR.Api.Modules.Security.Entities;

namespace OVOSUR.Api.Modules.Employees.Entities;

public sealed class Empleado
{
    public int EmpleadoId { get; set; }
    public Guid UsuarioId { get; set; }
    public string? CodigoEmpleado { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string? Area { get; set; }
    public string? Cargo { get; set; }
    public bool Activo { get; set; } = true;

    public Usuario Usuario { get; set; } = null!;
}
