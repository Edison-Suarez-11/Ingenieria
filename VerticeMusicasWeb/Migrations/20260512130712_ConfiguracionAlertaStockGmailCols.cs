using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VerticeMuiscaWeb.Migrations
{
    /// <inheritdoc />
    public partial class ConfiguracionAlertaStockGmailCols : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "claveAppGmail",
                table: "ConfiguracionAlertaStock",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "correoGmailParaEnviar",
                table: "ConfiguracionAlertaStock",
                type: "TEXT",
                maxLength: 320,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "claveAppGmail",
                table: "ConfiguracionAlertaStock");

            migrationBuilder.DropColumn(
                name: "correoGmailParaEnviar",
                table: "ConfiguracionAlertaStock");
        }
    }
}
