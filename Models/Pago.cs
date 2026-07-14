using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

public class Pago
{
    public int Id { get; set; }
    public int ExpensaId { get; set; }
    public DateTime FechaPago { get; set; }
    public decimal MontoPagado { get; set; }
    public string MedioPago { get; set; } = string.Empty;
    public string? NumeroOperacion { get; set; }
    public string? BancoEntidad { get; set; }
    [ValidateNever]
    public string ComprobantePath { get; set; } = string.Empty;
    public string? Comentarios { get; set; }
    public EstadoPago Estado { get; set; } = EstadoPago.Aprobado;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public int? UnidadFuncionalId { get; set; }
    [ValidateNever]
    public UnidadFuncional? UnidadFuncional { get; set; } = null!;

    [ValidateNever]
    public Expensa Expensa { get; set; } = null!;
}

public enum EstadoPago
{
    PendienteRevision,
    Aprobado,
    Rechazado
}
