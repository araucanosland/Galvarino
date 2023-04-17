using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Application
{
    public class ReporteGestion
    {
        public string Periodo { get; set; }
        public DateTime FechaProc { get; set; }
        public string Folio_Credito { get; set; }
        public DateTime FechaColocacion { get; set; }
        public int RutAfiliado { get; set; }
        public string DvRutAfiliado { get; set; }
        public string TipoCredito { get; set; }
        public string IdOficinaEvaluacion { get; set; }
        public string OficinaEvaluacion { get; set; }
        public string IdOficinaPago { get; set; }
        public string OficinaPago { get; set; }
        public string IdOficinaLegalizacion { get; set; }
        public string OficinaLegalizacion { get; set; }
        public string DocumentoRequerido1 { get; set; }
        public string DocumentoRequerido2 { get; set; }
        public int IdEstadoFolioGalvarino { get; set; }
        public string EstadoFolioGalvarino { get; set; }
        public DateTime FechaEtapaActual { get; set; }
        public string AreaRespEtapa { get; set; }
        public string FolioSKP { get; set; }
        public string CodigoValija { get; set; }
        public string TipoVenta { get; set; }

    }

}
