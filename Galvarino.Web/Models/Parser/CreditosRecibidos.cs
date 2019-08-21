using TinyCsvParser.Mapping;

namespace Galvarino.Web.Models.Parser
{
    public class CreditosRecibidos
    {
        public string Folio { get; set; }
        public string Rut { get; set; }
    }

    public class CsvCreditosRecibidosMapping : CsvMapping<CreditosRecibidos>
    {
        public CsvCreditosRecibidosMapping() : base()
        {
            MapProperty(0, x => x.Folio);
            MapProperty(1, x => x.Rut);
        }
    }
}