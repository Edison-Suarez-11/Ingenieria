using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerticeMuiscaWeb.Migrations
{
    /// <inheritdoc />
    public partial class AmpliarDatosProveedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "celular",
                table: "Proveedor",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ciudad",
                table: "Proveedor",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "correoElectronico",
                table: "Proveedor",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "direccion",
                table: "Proveedor",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nit",
                table: "Proveedor",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "personaContacto",
                table: "Proveedor",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "telefonoFijo",
                table: "Proveedor",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "celular",
                table: "Proveedor");

            migrationBuilder.DropColumn(
                name: "ciudad",
                table: "Proveedor");

            migrationBuilder.DropColumn(
                name: "correoElectronico",
                table: "Proveedor");

            migrationBuilder.DropColumn(
                name: "direccion",
                table: "Proveedor");

            migrationBuilder.DropColumn(
                name: "nit",
                table: "Proveedor");

            migrationBuilder.DropColumn(
                name: "personaContacto",
                table: "Proveedor");

            migrationBuilder.DropColumn(
                name: "telefonoFijo",
                table: "Proveedor");
        }
    }
}
