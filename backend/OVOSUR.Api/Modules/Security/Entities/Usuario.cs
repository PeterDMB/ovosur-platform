using OVOSUR.Api.Modules.Employees.Entities;
using OVOSUR.Api.Modules.Suppliers.Entities;

namespace OVOSUR.Api.Modules.Security.Entities;

public sealed class Usuario
{
    public Guid UsuarioId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string TipoUsuario { get; set; } = UserTypes.Interno;
    public string EstadoAprobacion { get; set; } = ApprovalStates.Pendiente;
    public int IntentosFallidos { get; set; }
    public DateTime? BloqueadoHasta { get; set; }
    public DateTime? UltimoAcceso { get; set; }
    public bool DebeCambiarPassword { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }

    public Empleado? Empleado { get; set; }
    public Proveedor? Proveedor { get; set; }
    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<AuditoriaEvento> AuditoriaEventos { get; set; } = [];
}
