using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Application
{
    public class ReporteProgramado
    {
        public int Id { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFinal { get; set; }
        public DateTime FechaEjecucion { get; set; }
        public string RutUsuario { get; set; }
        public string Estado { get; set; }
    }
}
