namespace Clinica.API.Models;

public class Evolucion
{
    public int IdEvolucion { get; set; }

    // Consulta derivada de la receta/tratamiento.
    // Se conserva para trazabilidad y compatibilidad.
    public int IdConsulta { get; set; }

    // Una evolución puede corresponder a una receta,
    // un tratamiento o ambos.
    public int? IdReceta { get; set; }

    public int? IdTratamiento { get; set; }

    public int IdEmpleado { get; set; }

    public DateTime Fecha { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public Consulta? Consulta { get; set; }

    public Receta? Receta { get; set; }

    public Tratamiento? Tratamiento { get; set; }

    public Empleado? Empleado { get; set; }
}