using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Galvarino.Web.Data.Migrations
{
    public partial class AuditorReasignacion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsRM",
                table: "Oficinas",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AudicionesReasignaciones",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    FechaAccion = table.Column<DateTime>(nullable: false),
                    UsuarioAccion = table.Column<string>(nullable: true),
                    TipoReasignacion = table.Column<string>(nullable: true),
                    AsignacionOriginal = table.Column<string>(nullable: true),
                    AsignacionNueva = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AudicionesReasignaciones", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AudicionesReasignaciones");

            migrationBuilder.DropColumn(
                name: "EsRM",
                table: "Oficinas");
        }
    }
}
