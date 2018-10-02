using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Application
{
    public class Documento
    {
        public int Id { get; set; }
        public string Resumen { get; set; }
        public string Codificacion { get; set; }
        public TipoDocumento TipoDocumento { get; set; }
        public ExpedienteCredito ExpedienteCredito { get; set; }

    }
}
