using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Galvarino.Web.Data.Migrations
{
    public partial class ValijaValoradaNueva : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ValijaValoradaId",
                table: "ExpedientesCreditos",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ValijasValoradas",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FechaEnvio = table.Column<DateTime>(nullable: false),
                    OficinaId = table.Column<int>(nullable: true),
                    CodigoSeguimiento = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValijasValoradas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValijasValoradas_Oficinas_OficinaId",
                        column: x => x.OficinaId,
                        principalTable: "Oficinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpedientesCreditos_ValijaValoradaId",
                table: "ExpedientesCreditos",
                column: "ValijaValoradaId");

            migrationBuilder.CreateIndex(
                name: "IX_ValijasValoradas_OficinaId",
                table: "ValijasValoradas",
                column: "OficinaId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpedientesCreditos_ValijasValoradas_ValijaValoradaId",
                table: "ExpedientesCreditos",
                column: "ValijaValoradaId",
                principalTable: "ValijasValoradas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpedientesCreditos_ValijasValoradas_ValijaValoradaId",
                table: "ExpedientesCreditos");

            migrationBuilder.DropTable(
                name: "ValijasValoradas");

            migrationBuilder.DropIndex(
                name: "IX_ExpedientesCreditos_ValijaValoradaId",
                table: "ExpedientesCreditos");

            migrationBuilder.DropColumn(
                name: "ValijaValoradaId",
                table: "ExpedientesCreditos");
        }
    }
}
