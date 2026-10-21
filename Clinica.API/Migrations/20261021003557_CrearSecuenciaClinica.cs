using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinica.API.Migrations
{
    /// <inheritdoc />
    public partial class CrearSecuenciaClinica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdDiagnostico",
                table: "Examenes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdReceta",
                table: "Evoluciones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdTratamiento",
                table: "Evoluciones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdDiagnostico",
                table: "AsignacionesHabitacion",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Examenes_IdDiagnostico",
                table: "Examenes",
                column: "IdDiagnostico");

            migrationBuilder.CreateIndex(
                name: "IX_Evoluciones_IdReceta",
                table: "Evoluciones",
                column: "IdReceta");

            migrationBuilder.CreateIndex(
                name: "IX_Evoluciones_IdTratamiento",
                table: "Evoluciones",
                column: "IdTratamiento");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesHabitacion_IdDiagnostico",
                table: "AsignacionesHabitacion",
                column: "IdDiagnostico");

            migrationBuilder.AddForeignKey(
                name: "FK_AsignacionesHabitacion_Diagnosticos_IdDiagnostico",
                table: "AsignacionesHabitacion",
                column: "IdDiagnostico",
                principalTable: "Diagnosticos",
                principalColumn: "IdDiagnostico",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Evoluciones_Recetas_IdReceta",
                table: "Evoluciones",
                column: "IdReceta",
                principalTable: "Recetas",
                principalColumn: "IdReceta",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Evoluciones_Tratamientos_IdTratamiento",
                table: "Evoluciones",
                column: "IdTratamiento",
                principalTable: "Tratamientos",
                principalColumn: "IdTratamiento",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Examenes_Diagnosticos_IdDiagnostico",
                table: "Examenes",
                column: "IdDiagnostico",
                principalTable: "Diagnosticos",
                principalColumn: "IdDiagnostico",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AsignacionesHabitacion_Diagnosticos_IdDiagnostico",
                table: "AsignacionesHabitacion");

            migrationBuilder.DropForeignKey(
                name: "FK_Evoluciones_Recetas_IdReceta",
                table: "Evoluciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Evoluciones_Tratamientos_IdTratamiento",
                table: "Evoluciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Examenes_Diagnosticos_IdDiagnostico",
                table: "Examenes");

            migrationBuilder.DropIndex(
                name: "IX_Examenes_IdDiagnostico",
                table: "Examenes");

            migrationBuilder.DropIndex(
                name: "IX_Evoluciones_IdReceta",
                table: "Evoluciones");

            migrationBuilder.DropIndex(
                name: "IX_Evoluciones_IdTratamiento",
                table: "Evoluciones");

            migrationBuilder.DropIndex(
                name: "IX_AsignacionesHabitacion_IdDiagnostico",
                table: "AsignacionesHabitacion");

            migrationBuilder.DropColumn(
                name: "IdDiagnostico",
                table: "Examenes");

            migrationBuilder.DropColumn(
                name: "IdReceta",
                table: "Evoluciones");

            migrationBuilder.DropColumn(
                name: "IdTratamiento",
                table: "Evoluciones");

            migrationBuilder.DropColumn(
                name: "IdDiagnostico",
                table: "AsignacionesHabitacion");
        }
    }
}
