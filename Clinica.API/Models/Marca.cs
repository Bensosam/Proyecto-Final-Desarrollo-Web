namespace Clinica.API.Models;

public class Marca
{
    public int IdMarca { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public bool Estado { get; set; } = true;

    public ICollection<Medicamento> Medicamentos { get; set; }
        = new List<Medicamento>();
}
