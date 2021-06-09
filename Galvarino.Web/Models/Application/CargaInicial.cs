using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Application
{
    public class CargaInicial
    {
        public int Id { get; set; }
        public DateTime FechaCarga { get; set; }
        public DateTime FechaCorresponde { get; set; }
        public string FolioCredito { get; set; }
        public string RutAfiliado { get; set; }
        public string CodigoOficinaIngreso { get; set; }
        public string CodigoOficinaPago { get; set; }
        public string LineaCredito { get; set; }
        public string RutResponsable { get; set; }
        public string CanalVenta { get; set; }
        public string Estado { get; set; }
        public string FechaVigencia { get; set; }
        public string NombreArchivoCarga { get; set; }

        public string TipoSegmento { get; set; }
        public string SeguroCesantia { get; set; }
        public string Afecto { get; set; }
        public string Aval { get; set; }
        public string SeguroDesgravamen { get; set; }
        //public string NroOferta { get; set; }
        //public string TipoVenta { get; set; }
        //public string FormaPago { get; set; }
        //public string CompraCartera { get; set; }
        //public string DigitalizarSegDesgr { get; set; }
        //public string DigitalizarSegCesantia { get; set; }


    }
}
