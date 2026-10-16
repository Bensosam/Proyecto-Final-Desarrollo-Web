namespace Clinica.API.DTOs;

public class AplicarPermisosRolDto
{
    public List<PermisosModuloDto> Modulos { get; set; } = [];
}

public class PermisosModuloDto
{
    public int IdModulo { get; set; }

    public List<int> IdsOperaciones { get; set; } = [];
}