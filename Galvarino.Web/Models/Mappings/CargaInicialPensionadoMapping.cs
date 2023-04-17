using Galvarino.Web.Models.Application.Pensionado;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TinyCsvParser.Mapping;

namespace Galvarino.Web.Models.Mappings
{
    public class CargaInicialPensionado
    {

        public int Id { get; set; }
        public DateTime FechaCarga { get; set; }
        public DateTime FechaProceso { get; set; }
        public string Folio { get; set; }
        public string Estado { get; set; }
        public string RutPensionado { get; set; }
        public string DvPensionado { get; set; }
        public string NombrePensionado { get; set; }
        public int IdTipo { get; set; }
        public string Tipo { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public DateTime FechaEfectiva { get; set; }
        public int IdSucursal { get; set; }
        public string Sucursal { get; set; }
        public string Forma { get; set; }
        public string TipoMovimiento { get; set; }

    }

    public class CargaInicialPensionadoMapping : CsvMapping<CargaInicialPensionado>
    {
        public CargaInicialPensionadoMapping()
            : base()
        {

            MapProperty(1, x => x.FechaProceso);
            //MapProperty(1, x => x.FolioCredito);
            //MapProperty(2, x => x.CodigoOficinaIngreso);
            //MapProperty(4, x => x.CodigoOficinaPago);
            //MapProperty(6, x => x.Estado);
            //MapProperty(7, x => x.LineaCredito);
            //MapProperty(8, x => x.RutResponsable);
            //MapProperty(9, x => x.CanalVenta);
            //MapProperty(10, x => x.FechaCorresponde);
            //MapProperty(13, x => x.SeguroCesantia);
            //MapProperty(14, x => x.Afecto);
            //MapProperty(15, x => x.Aval);
            //MapProperty(16, x => x.SeguroDesgravamen);
            //MapProperty(17, x => x.TipoVenta);
        }


    }



   
}
