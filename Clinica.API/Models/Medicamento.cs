namespace Clinica.API.Models;

public class Medicamento
{
    public int IdMedicamento { get; set; }

    public int IdCategoria { get; set; }

    public int IdMarca { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public decimal PrecioVenta { get; set; }

    public string? Imagen { get; set; }

    public bool Estado { get; set; }

    public Categoria? Categoria { get; set; }

    public Marca? Marca { get; set; }

    public ICollection<LoteMedicamento> Lotes { get; set; }
        = new List<LoteMedicamento>();
}