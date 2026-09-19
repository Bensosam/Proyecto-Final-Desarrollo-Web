namespace Clinica.API.Models;

public class Evolucion
{
    public int IdEvolucion { get; set; }

    public int IdConsulta { get; set; }

    public int IdEmpleado { get; set; }

    public DateTime Fecha { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public Consulta? Consulta { get; set; }

    public Empleado? Empleado { get; set; }
}