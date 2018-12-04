using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Galvarino.Web.Data.Migrations
{
    public partial class GestionPagaresSC : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GestionPagaresSinCustodia",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 40, nullable: false),
                    PagareSinCustodiaId = table.Column<string>(nullable: true),
                    FechaGestion = table.Column<DateTime>(nullable: false),
                    Estado = table.Column<string>(nullable: true),
                    Resumen = table.Column<string>(maxLength: 500, nullable: true),
                    EjecutadoPor = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GestionPagaresSinCustodia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GestionPagaresSinCustodia_PagaresSinCustodia_PagareSinCustodiaId",
                        column: x => x.PagareSinCustodiaId,
                        principalTable: "PagaresSinCustodia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GestionPagaresSinCustodia_PagareSinCustodiaId",
                table: "GestionPagaresSinCustodia",
                column: "PagareSinCustodiaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GestionPagaresSinCustodia");
        }
    }
}
