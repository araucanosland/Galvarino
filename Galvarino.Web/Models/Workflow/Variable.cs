using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Workflow
{
    public class Variable
    {
        public int Id { get; set; }
        public string NumeroTicket { get; set; }
        public string Clave { get; set; }
        public string Valor { get; set; }
        public string Tipo { get; set; }
    }
}
