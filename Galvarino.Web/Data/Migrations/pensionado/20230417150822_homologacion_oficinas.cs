using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Galvarino.Web.data.Migrations.pensionado
{
    public partial class homologacion_oficinas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "pensionado");

            migrationBuilder.CreateTable(
                name: "HomologacionOficinas",
                schema: "pensionado",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IdSucursalActividad = table.Column<string>(type: "varchar(20)", nullable: true),
                    Codificacion = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomologacionOficinas", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HomologacionOficinas",
                schema: "pensionado");
        }
    }
}
