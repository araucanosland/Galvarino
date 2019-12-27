using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Application
{
    public class ExpedienteComplementario
    {
        public int Id { get; set; }
        public int CreditoId { get; set; }
        public string FolioCredito { get; set; }
        public string NumeroTicket { get; set; }
        public string CodigoSeguimiento { get; set; }


    }
}
