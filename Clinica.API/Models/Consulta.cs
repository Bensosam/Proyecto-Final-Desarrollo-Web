namespace Clinica.API.Models;

public class Consulta
{
    public int IdConsulta { get; set; }

    public int IdPaciente { get; set; }

    public int IdEmpleado { get; set; }

    public DateTime FechaConsulta { get; set; }

    public string Motivo { get; set; } = string.Empty;

    public string Observaciones { get; set; } = string.Empty;

    public Paciente? Paciente { get; set; }

    public Empleado? Empleado { get; set; }

    public ICollection<Diagnostico> Diagnosticos { get; set; }
        = new List<Diagnostico>();

    public ICollection<Tratamiento> Tratamientos { get; set; }
        = new List<Tratamiento>();

    public ICollection<Examen> Examenes { get; set; }
        = new List<Examen>();

    public ICollection<Evolucion> Evoluciones { get; set; }
        = new List<Evolucion>();
}