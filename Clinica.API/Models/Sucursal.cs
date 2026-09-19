namespace Clinica.API.Models;

public class Sucursal
{
    public int IdSucursal { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Direccion { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;

    public bool Estado { get; set; }

    public ICollection<Habitacion> Habitaciones { get; set; }
        = new List<Habitacion>();

    public ICollection<Empleado> Empleados { get; set; }
        = new List<Empleado>();
}