namespace Clinica.API.Models;

public class AsignacionHabitacion
{
    public int IdAsignacion { get; set; }

    public int IdPaciente { get; set; }

    public int IdHabitacion { get; set; }

    public DateTime FechaIngreso { get; set; }

    public DateTime? FechaEgreso { get; set; }

    public string Estado { get; set; } = "Activa";

    public Paciente? Paciente { get; set; }

    public Habitacion? Habitacion { get; set; }
}