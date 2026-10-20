using Microsoft.EntityFrameworkCore.Migrations;


#nullable disable

namespace Clinica.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarNumeroConsultaPaciente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Consultas_IdPaciente",
                table: "Consultas");

            // Primero agregamos la columna.
            // Temporalmente las consultas existentes reciben 0.
            migrationBuilder.AddColumn<int>(
                name: "NumeroConsultaPaciente",
                table: "Consultas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Numerar automáticamente las consultas existentes
            // de cada paciente.
            //
            // El conteo comienza nuevamente en 1 para cada paciente
            // y respeta el orden cronológico de las consultas.
            migrationBuilder.Sql(
                """
                WITH ConsultasNumeradas AS
                (
                    SELECT
                        IdConsulta,
                        ROW_NUMBER() OVER
                        (
                            PARTITION BY IdPaciente
                            ORDER BY FechaConsulta, IdConsulta
                        ) AS Numero
                    FROM Consultas
                )
                UPDATE C
                SET C.NumeroConsultaPaciente = CN.Numero
                FROM Consultas AS C
                INNER JOIN ConsultasNumeradas AS CN
                    ON C.IdConsulta = CN.IdConsulta;
                """
            );

            // Una vez numerados los registros existentes,
            // garantizamos que un mismo paciente no pueda
            // tener dos consultas con el mismo número.
            migrationBuilder.CreateIndex(
                name: "IX_Consultas_IdPaciente_NumeroConsultaPaciente",
                table: "Consultas",
                columns: new[]
                {
                    "IdPaciente",
                    "NumeroConsultaPaciente"
                },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Consultas_IdPaciente_NumeroConsultaPaciente",
                table: "Consultas");

            migrationBuilder.DropColumn(
                name: "NumeroConsultaPaciente",
                table: "Consultas");

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_IdPaciente",
                table: "Consultas",
                column: "IdPaciente");
        }
    }
}
