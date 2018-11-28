using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Application
{
    public class AuditorReasignacion
    {
        public string Id { get; set; }
        public DateTime FechaAccion { get; set; }
        public string UsuarioAccion { get; set; }
        public string TipoReasignacion { get; set; }
        public string AsignacionOriginal { get; set; }
        public string AsignacionNueva { get; set; }
    }
}
