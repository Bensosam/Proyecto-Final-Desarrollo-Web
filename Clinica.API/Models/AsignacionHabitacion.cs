namespace Clinica.API.Models;

public class AsignacionHabitacion
{
    public int IdAsignacion { get; set; }

    public int IdPaciente { get; set; }

    // Diagnóstico que origina la hospitalización.
    // Nullable únicamente para conservar hospitalizaciones antiguas.
    public int? IdDiagnostico { get; set; }

    public int IdHabitacion { get; set; }

    public DateTime FechaIngreso { get; set; }

    public DateTime? FechaEgreso { get; set; }

    public string Estado { get; set; } = "Activa";

    public Paciente? Paciente { get; set; }

    public Diagnostico? Diagnostico { get; set; }

    public Habitacion? Habitacion { get; set; }
}