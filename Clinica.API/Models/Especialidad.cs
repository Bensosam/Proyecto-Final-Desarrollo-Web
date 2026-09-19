namespace Clinica.API.Models;

public class Especialidad
{
    public int IdEspecialidad { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public ICollection<Empleado> Empleados { get; set; }
        = new List<Empleado>();
}