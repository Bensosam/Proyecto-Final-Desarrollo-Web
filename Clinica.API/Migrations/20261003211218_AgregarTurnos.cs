using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinica.API.Migrations
{
    public partial class AgregarTurnos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Turnos",
                columns: table => new
                {
                    IdTurno = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeSpan>(type: "time", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turnos", x => x.IdTurno);
                });

            migrationBuilder.InsertData(
                table: "Turnos",
                columns: new[] { "IdTurno", "Nombre", "HoraInicio", "HoraFin", "Estado" },
                values: new object[,]
                {
                    { 1, "Matutino", new TimeSpan(7, 0, 0), new TimeSpan(15, 0, 0), true },
                    { 2, "Vespertino", new TimeSpan(15, 0, 0), new TimeSpan(23, 0, 0), true },
                    { 3, "Nocturno", new TimeSpan(23, 0, 0), new TimeSpan(7, 0, 0), true }
                });

            migrationBuilder.AddColumn<int>(
                name: "IdTurno",
                table: "Empleados",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_IdTurno",
                table: "Empleados",
                column: "IdTurno");

            migrationBuilder.AddForeignKey(
                name: "FK_Empleados_Turnos_IdTurno",
                table: "Empleados",
                column: "IdTurno",
                principalTable: "Turnos",
                principalColumn: "IdTurno",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Empleados_Turnos_IdTurno",
                table: "Empleados");

            migrationBuilder.DropIndex(
                name: "IX_Empleados_IdTurno",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "IdTurno",
                table: "Empleados");

            migrationBuilder.DropTable(
                name: "Turnos");
        }
    }
}
