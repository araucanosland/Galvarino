using Microsoft.EntityFrameworkCore.Migrations;

namespace Galvarino.Web.data.Migrations.pensionado
{
    public partial class modificarhomologacionoficinas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Codificacion",
                schema: "pensionado",
                table: "HomologacionOficinas");

            migrationBuilder.AddColumn<int>(
                name: "IdOficina",
                schema: "pensionado",
                table: "HomologacionOficinas",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdOficina",
                schema: "pensionado",
                table: "HomologacionOficinas");

            migrationBuilder.AddColumn<string>(
                name: "Codificacion",
                schema: "pensionado",
                table: "HomologacionOficinas",
                type: "varchar(20)",
                nullable: true);
        }
    }
}
