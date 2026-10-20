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

            migrationBuilder.AddColumn<int>(
                name: "NumeroConsultaPaciente",
                table: "Consultas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_IdPaciente_NumeroConsultaPaciente",
                table: "Consultas",
                columns: new[] { "IdPaciente", "NumeroConsultaPaciente" },
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
