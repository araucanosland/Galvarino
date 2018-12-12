using Microsoft.EntityFrameworkCore.Migrations;

namespace Galvarino.Web.Data.Migrations
{
    public partial class IndexesPt2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "NumeroTicket",
                table: "Creditos",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Creditos_NumeroTicket",
                table: "Creditos",
                column: "NumeroTicket");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Creditos_NumeroTicket",
                table: "Creditos");

            migrationBuilder.AlterColumn<string>(
                name: "NumeroTicket",
                table: "Creditos",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
