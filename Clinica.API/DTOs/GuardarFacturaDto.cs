namespace Clinica.API.DTOs;

public class GuardarFacturaDto
{
    public int? IdPaciente { get; set; }

    public int? IdVenta { get; set; }

    public List<ItemFacturaDto> Items { get; set; } = [];

    public List<int> IdsAsignacionesHabitacion { get; set; } = [];
}

public class ItemFacturaDto
{
    public int IdCatalogo { get; set; }

    public int Cantidad { get; set; }
}

public class ActualizarEstadoFacturaDto
{
    public string Estado { get; set; } = string.Empty;
}