using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Galvarino.Web.Data.Migrations
{
    public partial class ValijaOficinasEnt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ValijaOficinaId",
                table: "ExpedientesCreditos",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ValijasOficinas",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FechaEnvio = table.Column<DateTime>(nullable: false),
                    OficinaEnvioId = table.Column<int>(nullable: true),
                    OficinaDestinoId = table.Column<int>(nullable: true),
                    CodigoSeguimiento = table.Column<string>(nullable: true),
                    MarcaAvance = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValijasOficinas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValijasOficinas_Oficinas_OficinaDestinoId",
                        column: x => x.OficinaDestinoId,
                        principalTable: "Oficinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ValijasOficinas_Oficinas_OficinaEnvioId",
                        column: x => x.OficinaEnvioId,
                        principalTable: "Oficinas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpedientesCreditos_ValijaOficinaId",
                table: "ExpedientesCreditos",
                column: "ValijaOficinaId");

            migrationBuilder.CreateIndex(
                name: "IX_ValijasOficinas_OficinaDestinoId",
                table: "ValijasOficinas",
                column: "OficinaDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_ValijasOficinas_OficinaEnvioId",
                table: "ValijasOficinas",
                column: "OficinaEnvioId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpedientesCreditos_ValijasOficinas_ValijaOficinaId",
                table: "ExpedientesCreditos",
                column: "ValijaOficinaId",
                principalTable: "ValijasOficinas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpedientesCreditos_ValijasOficinas_ValijaOficinaId",
                table: "ExpedientesCreditos");

            migrationBuilder.DropTable(
                name: "ValijasOficinas");

            migrationBuilder.DropIndex(
                name: "IX_ExpedientesCreditos_ValijaOficinaId",
                table: "ExpedientesCreditos");

            migrationBuilder.DropColumn(
                name: "ValijaOficinaId",
                table: "ExpedientesCreditos");
        }
    }
}
