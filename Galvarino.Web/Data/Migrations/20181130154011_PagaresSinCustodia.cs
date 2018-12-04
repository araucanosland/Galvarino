using Microsoft.EntityFrameworkCore.Migrations;

namespace Galvarino.Web.Data.Migrations
{
    public partial class PagaresSinCustodia : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PagaresSinCustodia",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    FolioCreditoStr = table.Column<string>(nullable: true),
                    CodigoOficina = table.Column<string>(nullable: true),
                    NombreOficina = table.Column<string>(nullable: true),
                    Zona = table.Column<string>(nullable: true),
                    FolioCredito = table.Column<string>(nullable: true),
                    LineaCredito = table.Column<string>(nullable: true),
                    TipoCredito = table.Column<string>(nullable: true),
                    RutAfiliado = table.Column<string>(nullable: true),
                    DvAfiliado = table.Column<string>(nullable: true),
                    Segmento = table.Column<string>(nullable: true),
                    FechaColocacion = table.Column<string>(nullable: true),
                    Ano = table.Column<string>(nullable: true),
                    Mes = table.Column<string>(nullable: true),
                    Plazo = table.Column<string>(nullable: true),
                    Castigo = table.Column<string>(nullable: true),
                    KEfectivo = table.Column<string>(nullable: true),
                    IDevMensual = table.Column<string>(nullable: true),
                    MontoBruto = table.Column<string>(nullable: true),
                    MontoNeto = table.Column<string>(nullable: true),
                    KCalculado = table.Column<string>(nullable: true),
                    SaldoMigrado = table.Column<string>(nullable: true),
                    MesesMorosos = table.Column<string>(nullable: true),
                    NumCuotasMorosas = table.Column<string>(nullable: true),
                    NumCuotasCastigadas = table.Column<string>(nullable: true),
                    TipoFinanciamiento = table.Column<string>(nullable: true),
                    Estado = table.Column<string>(nullable: true),
                    MarcaRenRep = table.Column<string>(nullable: true),
                    Estadodocto = table.Column<string>(nullable: true),
                    Estadoproceso = table.Column<string>(nullable: true),
                    Estado1 = table.Column<string>(nullable: true),
                    CodigoSucursalPago = table.Column<string>(nullable: true),
                    NombreSucursalPago = table.Column<string>(nullable: true),
                    FolioCorregido = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagaresSinCustodia", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PagaresSinCustodia");
        }
    }
}
