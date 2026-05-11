namespace OVOSUR.Api.Modules.Suppliers.Entities;

public sealed class ProveedorDocumento
{
    public long ProveedorDocumentoId { get; set; }
    public int ProveedorId { get; set; }
    public string TipoDocumento { get; set; } = string.Empty;
    public string NombreArchivoOriginal { get; set; } = string.Empty;
    public string RutaFisica { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public string EstadoRevision { get; set; } = "PENDIENTE";
    public DateTime FechaCarga { get; set; }

    public Proveedor Proveedor { get; set; } = null!;
}
