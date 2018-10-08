using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Application
{
    public class ExpedienteCredito
    {
        public int Id { get; set; }
        public DateTime FechaCreacion { get; set; }
        public Credito Credito { get; set; }
        public TipoExpediente TipoExpediente { get; set; }
        public PackNotaria PackNotaria { get; set; }
        public ICollection<Documento> Documentos { get; set; }
        public string NumeroSeguimiento { get; set; }

        public ExpedienteCredito()
        {
            Documentos = new List<Documento>();
        }
    }
}
