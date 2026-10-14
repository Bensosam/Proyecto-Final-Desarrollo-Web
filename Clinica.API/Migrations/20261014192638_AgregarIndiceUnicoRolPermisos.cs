using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinica.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarIndiceUnicoRolPermisos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RolPermisos_IdRol",
                table: "RolPermisos");

            migrationBuilder.CreateIndex(
                name: "IX_RolPermisos_IdRol_IdModulo_IdOperacion",
                table: "RolPermisos",
                columns: new[] { "IdRol", "IdModulo", "IdOperacion" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RolPermisos_IdRol_IdModulo_IdOperacion",
                table: "RolPermisos");

            migrationBuilder.CreateIndex(
                name: "IX_RolPermisos_IdRol",
                table: "RolPermisos",
                column: "IdRol");
        }
    }
}
