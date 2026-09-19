namespace Clinica.API.Models;

public class Habitacion
{
    public int IdHabitacion { get; set; }

    public int IdSucursal { get; set; }

    public string Numero { get; set; } = string.Empty;

    public string Estado { get; set; } = "Libre";

    public Sucursal? Sucursal { get; set; }

    public ICollection<AsignacionHabitacion> Asignaciones { get; set; }
        = new List<AsignacionHabitacion>();
}