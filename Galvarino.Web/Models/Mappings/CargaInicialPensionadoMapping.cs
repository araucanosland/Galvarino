using Galvarino.Web.Models.Application.Pensionado;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TinyCsvParser.Mapping;

namespace Galvarino.Web.Models.Mappings
{
    public class CargaInicialPensionadoIM
    {

        public int Id { get; set; }
        public DateTime FechaCarga { get; set; }
        public DateTime FechaProceso { get; set; }
        public string Folio { get; set; }
        public string Estado { get; set; }
        public string RutPensionado { get; set; }
        public string DvPensionado { get; set; }
        public string NombrePensionado { get; set; }
        public string Nombre2Pensionado { get; set; }
        public string ApellidoPatPensionado { get; set; }
        public string ApellidoMatPensionado { get; set; }
        public int IdTipo { get; set; }
        public string Tipo { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public DateTime FechaEfectiva { get; set; }
        public string IdSucursal { get; set; }
        public string Sucursal { get; set; }
        public string Forma { get; set; }
        public string TipoMovimiento { get; set; }

    }

    public class CargaInicialPensionadoMapping : CsvMapping<CargaInicialPensionadoIM>
    {
        public CargaInicialPensionadoMapping()
            : base()
        {

            MapProperty(1, x => x.FechaProceso);
            MapProperty(2, x => x.Folio);
            MapProperty(5, x => x.Estado);
            MapProperty(9, x => x.RutPensionado);
            MapProperty(10, x => x.DvPensionado);
            MapProperty(11, x => x.NombrePensionado);
            MapProperty(12, x => x.Nombre2Pensionado);
            MapProperty(13, x => x.ApellidoPatPensionado);
            MapProperty(14, x => x.ApellidoMatPensionado);
            MapProperty(31, x => x.FechaSolicitud);
            MapProperty(40, x => x.IdSucursal);
            MapProperty(41, x => x.Sucursal);
            MapProperty(55, x => x.FechaEfectiva);
            MapProperty(56, x => x.IdTipo);
            MapProperty(57, x => x.Tipo);
        }


    }



   
}
