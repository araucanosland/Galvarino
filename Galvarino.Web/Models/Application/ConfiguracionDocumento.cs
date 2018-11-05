using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Application
{
    public class ConfiguracionDocumento
    {
        public int Id { get; set; }
        public TipoDocumento TipoDocumento { get; set; }
        public TipoExpediente TipoExpediente { get; set; }
        public TipoCredito TipoCredito { get; set; }
        public string Codificacion { get; set; }

    }
}
