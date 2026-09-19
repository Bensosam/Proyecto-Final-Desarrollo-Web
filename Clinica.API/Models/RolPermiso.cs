namespace Clinica.API.Models;

public class RolPermiso
{
    public int IdRolPermiso { get; set; }

    public int IdRol { get; set; }

    public int IdModulo { get; set; }

    public int IdOperacion { get; set; }

    public Rol? Rol { get; set; }

    public Modulo? Modulo { get; set; }

    public Operacion? Operacion { get; set; }
}