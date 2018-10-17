using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Galvarino.Web.Data.Migrations
{
    public partial class NuevaCajaValorada : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CajaValoradaId",
                table: "ExpedientesCreditos",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CajasValoradas",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FechaEnvio = table.Column<DateTime>(nullable: false),
                    CodigoSeguimiento = table.Column<string>(nullable: true),
                    MarcaAvance = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CajasValoradas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpedientesCreditos_CajaValoradaId",
                table: "ExpedientesCreditos",
                column: "CajaValoradaId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpedientesCreditos_CajasValoradas_CajaValoradaId",
                table: "ExpedientesCreditos",
                column: "CajaValoradaId",
                principalTable: "CajasValoradas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpedientesCreditos_CajasValoradas_CajaValoradaId",
                table: "ExpedientesCreditos");

            migrationBuilder.DropTable(
                name: "CajasValoradas");

            migrationBuilder.DropIndex(
                name: "IX_ExpedientesCreditos_CajaValoradaId",
                table: "ExpedientesCreditos");

            migrationBuilder.DropColumn(
                name: "CajaValoradaId",
                table: "ExpedientesCreditos");
        }
    }
}
