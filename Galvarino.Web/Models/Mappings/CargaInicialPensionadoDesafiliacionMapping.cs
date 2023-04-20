using Galvarino.Web.Models.Application.Pensionado;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TinyCsvParser.Mapping;

namespace Galvarino.Web.Models.Mappings
{
    public class CargaInicialPensionadoDesafiliacionIM
    {

        public int Id { get; set; }
        public DateTime FechaCarga { get; set; }
        public string FechaProceso { get; set; }
        public string Folio { get; set; }
        public string Estado { get; set; }
        public string RutPensionado { get; set; }
        public string DvPensionado { get; set; }
        public string NombrePensionado { get; set; }
        public string IdTipo { get; set; }
        public string Tipo { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public DateTime FechaEfectiva { get; set; }
        public string IdSucursal { get; set; }
        public string Sucursal { get; set; }
        public string Forma { get; set; }
        public string TipoMovimiento { get; set; }

    }

    public class CargaInicialPensionadoDesafiliacionMapping : CsvMapping<CargaInicialPensionadoDesafiliacionIM>
    {
        public CargaInicialPensionadoDesafiliacionMapping()
            : base()
        {

            MapProperty(1, x => x.FechaProceso);
            MapProperty(2, x => x.Folio);
            MapProperty(4, x => x.Estado);
            MapProperty(5, x => x.RutPensionado);
            MapProperty(6, x => x.DvPensionado);
            MapProperty(7, x => x.NombrePensionado); 
            MapProperty(8, x => x.IdTipo);
            MapProperty(9, x => x.Tipo);
            MapProperty(14, x => x.FechaSolicitud);
            MapProperty(15, x => x.FechaEfectiva);
            MapProperty(24, x => x.IdSucursal);
            MapProperty(27, x => x.Forma);
            

        }


    }



   
}
