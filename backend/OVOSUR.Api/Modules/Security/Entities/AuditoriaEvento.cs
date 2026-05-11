namespace OVOSUR.Api.Modules.Security.Entities;

public sealed class AuditoriaEvento
{
    public long AuditoriaEventoId { get; set; }
    public Guid? UsuarioId { get; set; }
    public string Evento { get; set; } = string.Empty;
    public string? Entidad { get; set; }
    public string? EntidadId { get; set; }
    public string? Ip { get; set; }
    public string? UserAgent { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime FechaEvento { get; set; }

    public Usuario? Usuario { get; set; }
}
