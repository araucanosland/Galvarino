using Microsoft.EntityFrameworkCore.Migrations;

namespace Galvarino.Web.Data.Migrations
{
    public partial class QuienAlmacenaDocumentosComerciales : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoOficina",
                table: "AlmacenajesComerciales",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RutEjecutivo",
                table: "AlmacenajesComerciales",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoOficina",
                table: "AlmacenajesComerciales");

            migrationBuilder.DropColumn(
                name: "RutEjecutivo",
                table: "AlmacenajesComerciales");
        }
    }
}
