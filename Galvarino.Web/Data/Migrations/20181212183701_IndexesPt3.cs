using Microsoft.EntityFrameworkCore.Migrations;

namespace Galvarino.Web.Data.Migrations
{
    public partial class IndexesPt3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NumeroTicket",
                table: "Variables",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Clave",
                table: "Variables",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Variables_NumeroTicket_Clave",
                table: "Variables",
                columns: new[] { "NumeroTicket", "Clave" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Variables_NumeroTicket_Clave",
                table: "Variables");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroTicket",
                table: "Variables",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Clave",
                table: "Variables",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
