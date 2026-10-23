using System.ComponentModel.DataAnnotations;

namespace Clinica.API.Models;

public class HistorialMedico
{
    [Key]
    public int IdHistorialMedico { get; set; }

    public int IdPaciente { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string AntecedentesPersonales { get; set; }
        = string.Empty;

    public string AntecedentesFamiliares { get; set; }
        = string.Empty;

    public string Alergias { get; set; }
        = string.Empty;

    public string EnfermedadesPrevias { get; set; }
        = string.Empty;

    public string Cirugias { get; set; }
        = string.Empty;

    public string MedicamentosHabituales { get; set; }
        = string.Empty;

    public string Habitos { get; set; }
        = string.Empty;

    public string ObservacionesEntrevistaInicial { get; set; }
        = string.Empty;

    public Paciente? Paciente { get; set; }
}
