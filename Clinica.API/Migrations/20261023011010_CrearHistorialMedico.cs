using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinica.API.Migrations
{
    /// <inheritdoc />
    public partial class CrearHistorialMedico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistorialesMedicos",
                columns: table => new
                {
                    IdHistorialMedico = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation(
                            "SqlServer:Identity",
                            "1, 1"),

                    IdPaciente = table.Column<int>(
                        type: "int",
                        nullable: false),

                    FechaCreacion = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),

                    AntecedentesPersonales = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: false),

                    AntecedentesFamiliares = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: false),

                    Alergias = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: false),

                    EnfermedadesPrevias = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: false),

                    Cirugias = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: false),

                    MedicamentosHabituales = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: false),

                    Habitos = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: false),

                    ObservacionesEntrevistaInicial = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_HistorialesMedicos",
                        x => x.IdHistorialMedico);

                    table.ForeignKey(
                        name: "FK_HistorialesMedicos_Pacientes_IdPaciente",
                        column: x => x.IdPaciente,
                        principalTable: "Pacientes",
                        principalColumn: "IdPaciente",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialesMedicos_IdPaciente",
                table: "HistorialesMedicos",
                column: "IdPaciente",
                unique: true);

            // Crear historial médico para todos los pacientes
            // que ya existían antes de esta migración.
            migrationBuilder.Sql(
                """
                INSERT INTO HistorialesMedicos
                (
                    IdPaciente,
                    FechaCreacion,
                    AntecedentesPersonales,
                    AntecedentesFamiliares,
                    Alergias,
                    EnfermedadesPrevias,
                    Cirugias,
                    MedicamentosHabituales,
                    Habitos,
                    ObservacionesEntrevistaInicial
                )
                SELECT
                    p.IdPaciente,
                    p.FechaRegistro,
                    '',
                    '',
                    '',
                    '',
                    '',
                    '',
                    '',
                    ''
                FROM Pacientes p
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM HistorialesMedicos h
                    WHERE h.IdPaciente = p.IdPaciente
                );
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistorialesMedicos");
        }
    }
}
