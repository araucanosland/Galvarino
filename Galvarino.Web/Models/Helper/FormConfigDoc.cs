using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Helper
{
    public class FormConfigDoc
    {
        public int Id { get; set; }
        public string Codificacion { get; set; }
        public string TipoCredito { get; set; }
        public string TipoExpediente { get; set; }
        public string TipoDocumento { get; set; }
    }
}