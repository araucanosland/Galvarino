using Microsoft.EntityFrameworkCore.Migrations;

namespace Galvarino.Web.data.migrations.pensionado
{
    public partial class CargasIniciales : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IdSucursalActividad",
                schema: "pensionado",
                table: "HomologacionOficinas",
                type: "varchar(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IdSucursalActividad",
                schema: "pensionado",
                table: "HomologacionOficinas",
                type: "varchar(20)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20)");
        }
    }
}
