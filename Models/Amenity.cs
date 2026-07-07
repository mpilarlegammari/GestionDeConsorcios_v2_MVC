using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

public class Amenity
{
    public int Id { get; set; }
    public int ConsorcioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Capacidad { get; set; }
    public bool Activo { get; set; } = true;

    [ValidateNever]
    public Consorcio Consorcio { get; set; } = null!;
    [ValidateNever]
    public List<Reserva> Reservas { get; set; } = new();
}
