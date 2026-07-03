using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

public class Comunicado
{
    public int Id { get; set; }
    public int ConsorcioId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;
    public string? ArchivoAdjuntoPath { get; set; }
    public bool Importante { get; set; }

    [ValidateNever]
    public Consorcio? Consorcio { get; set; } = null!;
}
