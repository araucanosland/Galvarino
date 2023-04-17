using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Galvarino.Web.data.migrations.pensionado
{
    public partial class CargasInicialesAgregarKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sucursal",
                schema: "pensionado",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SucursalDescripcion = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sucursal", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tipo",
                schema: "pensionado",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TipoDescripcion = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tipo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CargasIniciales",
                schema: "pensionado",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    FechaCarga = table.Column<DateTime>(nullable: false),
                    FechaProceso = table.Column<DateTime>(nullable: false),
                    Folio = table.Column<string>(nullable: true),
                    Estado = table.Column<string>(nullable: true),
                    RutPensionado = table.Column<string>(nullable: true),
                    DvPensionado = table.Column<string>(nullable: true),
                    NombrePensionado = table.Column<string>(nullable: true),
                    TipoId = table.Column<int>(nullable: true),
                    FechaSolicitud = table.Column<DateTime>(nullable: false),
                    FechaEfectiva = table.Column<DateTime>(nullable: false),
                    SucursalId = table.Column<int>(nullable: true),
                    Forma = table.Column<string>(nullable: true),
                    TipoMovimiento = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargasIniciales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CargasIniciales_Sucursal_SucursalId",
                        column: x => x.SucursalId,
                        principalSchema: "pensionado",
                        principalTable: "Sucursal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CargasIniciales_Tipo_TipoId",
                        column: x => x.TipoId,
                        principalSchema: "pensionado",
                        principalTable: "Tipo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CargasIniciales_SucursalId",
                schema: "pensionado",
                table: "CargasIniciales",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_CargasIniciales_TipoId",
                schema: "pensionado",
                table: "CargasIniciales",
                column: "TipoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CargasIniciales",
                schema: "pensionado");

            migrationBuilder.DropTable(
                name: "Sucursal",
                schema: "pensionado");

            migrationBuilder.DropTable(
                name: "Tipo",
                schema: "pensionado");
        }
    }
}
