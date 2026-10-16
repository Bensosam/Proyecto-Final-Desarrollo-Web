namespace Clinica.API.DTOs;

public class GuardarVentaDto
{
    public List<ItemVentaDto> Items { get; set; } = [];
}

public class ItemVentaDto
{
    public int IdLote { get; set; }

    public int Cantidad { get; set; }
}