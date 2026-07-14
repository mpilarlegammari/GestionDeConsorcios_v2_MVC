using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

public class Reserva
{
    public int Id { get; set; }
    public int AmenityId { get; set; }
    public int UnidadFuncionalId { get; set; }
    public DateTime FechaReserva { get; set; }
    public EstadoReserva Estado { get; set; } = EstadoReserva.Pendiente;
    public string? Observaciones { get; set; }
    public Turno? Turno { get; set; }
    [ValidateNever]
    public Amenity Amenity { get; set; } = null!;
    [ValidateNever]
    public UnidadFuncional UnidadFuncional { get; set; } = null!;
}

public enum EstadoReserva
{
    Pendiente,
    Confirmada,
    Cancelada,
    Rechazada
}

public enum Turno { 
Mañana,Tarde,Noche}
