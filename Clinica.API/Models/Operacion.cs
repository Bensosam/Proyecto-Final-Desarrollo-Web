namespace Clinica.API.Models;

public class Operacion
{
    public int IdOperacion { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public ICollection<RolPermiso> RolPermisos { get; set; }
        = new List<RolPermiso>();
}