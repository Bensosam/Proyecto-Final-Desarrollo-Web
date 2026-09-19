namespace Clinica.API.Models;

public class Modulo
{
    public int IdModulo { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public bool Estado { get; set; }

    public ICollection<RolPermiso> RolPermisos { get; set; }
        = new List<RolPermiso>();
}