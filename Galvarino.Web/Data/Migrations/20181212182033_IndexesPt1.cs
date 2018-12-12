using Microsoft.EntityFrameworkCore.Migrations;

namespace Galvarino.Web.Data.Migrations
{
    public partial class IndexesPt1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UnidadNegocioAsignada",
                table: "Tareas",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AsignadoA",
                table: "Tareas",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TipoUsuarioAsignado",
                table: "Etapas",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "TipoEtapa",
                table: "Etapas",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "NombreInterno",
                table: "Etapas",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_AsignadoA_Estado_UnidadNegocioAsignada",
                table: "Tareas",
                columns: new[] { "AsignadoA", "Estado", "UnidadNegocioAsignada" });

            migrationBuilder.CreateIndex(
                name: "IX_Etapas_NombreInterno_TipoEtapa_TipoUsuarioAsignado",
                table: "Etapas",
                columns: new[] { "NombreInterno", "TipoEtapa", "TipoUsuarioAsignado" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tareas_AsignadoA_Estado_UnidadNegocioAsignada",
                table: "Tareas");

            migrationBuilder.DropIndex(
                name: "IX_Etapas_NombreInterno_TipoEtapa_TipoUsuarioAsignado",
                table: "Etapas");

            migrationBuilder.AlterColumn<string>(
                name: "UnidadNegocioAsignada",
                table: "Tareas",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AsignadoA",
                table: "Tareas",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TipoUsuarioAsignado",
                table: "Etapas",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "TipoEtapa",
                table: "Etapas",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "NombreInterno",
                table: "Etapas",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
