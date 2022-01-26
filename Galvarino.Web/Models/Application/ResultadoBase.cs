using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Application
{
    public class ResultadoBase
    {
        public string Estado { get; set; }
        public string Mensaje { get; set; }
        public dynamic Objeto { get; set; }
    }
}
