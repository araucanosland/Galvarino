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
    
    }
}
