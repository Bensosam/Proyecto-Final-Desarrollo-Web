using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinica.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCatalogoFacturacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Catalogo",
                columns: table => new
                {
                    IdCatalogo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    IdMedicamento = table.Column<int>(type: "int", nullable: true),
                    IdTipoHabitacion = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalogo", x => x.IdCatalogo);
                    table.ForeignKey(
                        name: "FK_Catalogo_Medicamentos_IdMedicamento",
                        column: x => x.IdMedicamento,
                        principalTable: "Medicamentos",
                        principalColumn: "IdMedicamento",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Catalogo_TiposHabitacion_IdTipoHabitacion",
                        column: x => x.IdTipoHabitacion,
                        principalTable: "TiposHabitacion",
                        principalColumn: "IdTipoHabitacion",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Facturas",
                columns: table => new
                {
                    IdFactura = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeroFactura = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IdPaciente = table.Column<int>(type: "int", nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    FechaFactura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facturas", x => x.IdFactura);
                    table.ForeignKey(
                        name: "FK_Facturas_Pacientes_IdPaciente",
                        column: x => x.IdPaciente,
                        principalTable: "Pacientes",
                        principalColumn: "IdPaciente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Facturas_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetalleFactura",
                columns: table => new
                {
                    IdDetalleFactura = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdFactura = table.Column<int>(type: "int", nullable: false),
                    IdCatalogo = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleFactura", x => x.IdDetalleFactura);
                    table.ForeignKey(
                        name: "FK_DetalleFactura_Catalogo_IdCatalogo",
                        column: x => x.IdCatalogo,
                        principalTable: "Catalogo",
                        principalColumn: "IdCatalogo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetalleFactura_Facturas_IdFactura",
                        column: x => x.IdFactura,
                        principalTable: "Facturas",
                        principalColumn: "IdFactura",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Catalogo_Codigo",
                table: "Catalogo",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Catalogo_IdMedicamento",
                table: "Catalogo",
                column: "IdMedicamento");

            migrationBuilder.CreateIndex(
                name: "IX_Catalogo_IdTipoHabitacion",
                table: "Catalogo",
                column: "IdTipoHabitacion");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleFactura_IdCatalogo",
                table: "DetalleFactura",
                column: "IdCatalogo");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleFactura_IdFactura",
                table: "DetalleFactura",
                column: "IdFactura");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_IdPaciente",
                table: "Facturas",
                column: "IdPaciente");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_IdUsuario",
                table: "Facturas",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_NumeroFactura",
                table: "Facturas",
                column: "NumeroFactura",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetalleFactura");

            migrationBuilder.DropTable(
                name: "Catalogo");

            migrationBuilder.DropTable(
                name: "Facturas");
        }
    }
}
