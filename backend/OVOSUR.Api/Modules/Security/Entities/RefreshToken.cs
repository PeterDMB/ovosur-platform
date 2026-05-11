namespace OVOSUR.Api.Modules.Security.Entities;

public sealed class RefreshToken
{
    public long RefreshTokenId { get; set; }
    public Guid UsuarioId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime FechaExpiracion { get; set; }
    public DateTime? FechaRevocacion { get; set; }
    public string? IpCreacion { get; set; }
    public string? UserAgent { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
