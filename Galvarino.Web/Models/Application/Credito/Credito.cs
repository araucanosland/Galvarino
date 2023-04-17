using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Application
{
    public class Credito
    {
        public int Id { get; set; }
        public string FolioCredito { get; set; }
        public long MontoCredito { get; set; }
        public DateTime FechaFormaliza { get; set; }
        public DateTime FechaDesembolso { get; set; }
        public string RutCliente { get; set; }
        public string NombreCliente { get; set; }
        public string NumeroTicket { get; set; }
        public TipoCredito TipoCredito { get; set; }

    }
}
