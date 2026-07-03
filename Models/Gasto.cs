
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

public class Gasto
{
    public int Id { get; set; }
    public int ConsorcioId { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public decimal Monto { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public CategoriaGasto Categoria { get; set; } = CategoriaGasto.Otros;
    [ValidateNever]
    public string? ArchivoFacturaPath { get; set; }

    public string? Descripcion { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    [ValidateNever]
    public Consorcio? Consorcio { get; set; } = null!;
}

