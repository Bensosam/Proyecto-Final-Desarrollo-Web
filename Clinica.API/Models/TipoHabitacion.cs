namespace Clinica.API.Models;

public class TipoHabitacion
{
    public int IdTipoHabitacion { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public decimal PrecioPorDia { get; set; }

    public bool Estado { get; set; } = true;

    public ICollection<Habitacion> Habitaciones { get; set; }
        = new List<Habitacion>();
}