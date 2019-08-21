using TinyCsvParser.Mapping;

namespace Galvarino.Web.Models.Mappings
{

    public class CreditosRecibidosIM
    {
        public string Folio { get; set; }
        public string RutAfiliado { get; set; }
    }

    public class CreditosRecibidosMapping : CsvMapping<CreditosRecibidosIM>
    {
        public CreditosRecibidosMapping(): base(){
            MapProperty(0, x => x.Folio);
            MapProperty(1, x => x.RutAfiliado);
        }
    }

}