using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Galvarino.Web.Data.Migrations
{
    public partial class AlmacenajeComercial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AlmacenajeComercialId",
                table: "ExpedientesCreditos",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AlmacenajeComercial",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Fecha = table.Column<DateTime>(nullable: false),
                    CodigoSeguimiento = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlmacenajeComercial", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpedientesCreditos_AlmacenajeComercialId",
                table: "ExpedientesCreditos",
                column: "AlmacenajeComercialId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpedientesCreditos_AlmacenajeComercial_AlmacenajeComercialId",
                table: "ExpedientesCreditos",
                column: "AlmacenajeComercialId",
                principalTable: "AlmacenajeComercial",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpedientesCreditos_AlmacenajeComercial_AlmacenajeComercialId",
                table: "ExpedientesCreditos");

            migrationBuilder.DropTable(
                name: "AlmacenajeComercial");

            migrationBuilder.DropIndex(
                name: "IX_ExpedientesCreditos_AlmacenajeComercialId",
                table: "ExpedientesCreditos");

            migrationBuilder.DropColumn(
                name: "AlmacenajeComercialId",
                table: "ExpedientesCreditos");
        }
    }
}
