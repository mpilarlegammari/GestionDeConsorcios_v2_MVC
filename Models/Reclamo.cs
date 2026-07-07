
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

public class Reclamo
{
    public int Id { get; set; }
    public int UnidadFuncionalId { get; set; }
    public string Asunto { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public EstadoReclamo Estado { get; set; } = EstadoReclamo.Abierto;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaCierre { get; set; }
    public string? ObservacionAdministracion { get; set; }

    [ValidateNever]
    public UnidadFuncional UnidadFuncional { get; set; } = null!;
}

public enum EstadoReclamo
{
    Abierto,
    EnProceso,
    Cerrado
}
