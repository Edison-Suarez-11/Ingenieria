using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerticeMuiscaWeb.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablaProveedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Proveedor",
                columns: table => new
                {
                    idProveedor = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nombre = table.Column<string>(type: "TEXT", nullable: false),
                    contacto = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedor", x => x.idProveedor);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Proveedor_nombre",
                table: "Proveedor",
                column: "nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Proveedor");
        }
    }
}
