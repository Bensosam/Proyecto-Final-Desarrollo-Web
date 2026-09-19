namespace Clinica.API.Models;

public class Paciente
{
    public int IdPaciente { get; set; }

    public string Nombres { get; set; } = string.Empty;

    public string Apellidos { get; set; } = string.Empty;

    public string DPI { get; set; } = string.Empty;

    public DateTime FechaNacimiento { get; set; }

    public string Direccion { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; }

    public ICollection<AsignacionHabitacion> AsignacionesHabitacion { get; set; }
        = new List<AsignacionHabitacion>();

    public ICollection<Consulta> Consultas { get; set; }
        = new List<Consulta>();
}