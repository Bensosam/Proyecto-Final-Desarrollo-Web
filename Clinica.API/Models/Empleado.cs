namespace Clinica.API.Models;

public class Empleado
{
    public int IdEmpleado { get; set; }

    public int IdSucursal { get; set; }

    public int? IdEspecialidad { get; set; }

    public string Nombres { get; set; } = string.Empty;

    public string Apellidos { get; set; } = string.Empty;

    public string DPI { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public bool Estado { get; set; }

    public Sucursal? Sucursal { get; set; }

    public Especialidad? Especialidad { get; set; }

    public ICollection<Usuario> Usuarios { get; set; }
        = new List<Usuario>();

    public ICollection<Consulta> Consultas { get; set; }
        = new List<Consulta>();

    public ICollection<Evolucion> Evoluciones { get; set; }
        = new List<Evolucion>();
}