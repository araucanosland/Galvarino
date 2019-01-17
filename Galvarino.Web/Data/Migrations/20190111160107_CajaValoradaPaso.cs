using Microsoft.EntityFrameworkCore.Migrations;

namespace Galvarino.Web.Data.Migrations
{
    public partial class CajaValoradaPaso : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PasosValijasValoradas",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    CodigoCajaValorada = table.Column<string>(nullable: true),
                    FolioCredito = table.Column<string>(nullable: true),
                    FolioDocumento = table.Column<string>(nullable: true),
                    Usuario = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasosValijasValoradas", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasosValijasValoradas");
        }
    }
}
