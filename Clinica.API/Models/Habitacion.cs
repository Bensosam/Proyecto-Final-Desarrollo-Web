namespace Clinica.API.Models;

public class Habitacion
{
    public int IdHabitacion { get; set; }

    public int IdSucursal { get; set; }

    public int? IdTipoHabitacion { get; set; }

    public string Numero { get; set; } = string.Empty;

    public int CantidadCamas { get; set; } = 1;

    public string Estado { get; set; } = "Libre";

    public Sucursal? Sucursal { get; set; }

    public TipoHabitacion? TipoHabitacion { get; set; }

    public ICollection<AsignacionHabitacion> Asignaciones { get; set; }
        = new List<AsignacionHabitacion>();
}