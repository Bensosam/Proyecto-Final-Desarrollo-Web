using System.ComponentModel.DataAnnotations;

namespace Clinica.API.Models;

public class Turno
{
    [Key]
    public int IdTurno { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public TimeSpan HoraInicio { get; set; }

    public TimeSpan HoraFin { get; set; }

    public bool Estado { get; set; }

    public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
