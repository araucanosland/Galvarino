using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Web.Models.Security;

namespace Galvarino.Web.Models.Workflow
{
    public class Solicitud
    {
        public int Id { get; set; }
        public string NumeroTicket { get; set; }
        public Proceso Proceso { get; set; }
        public EstadoSolicitud Estado { get; set; }
        public string Resumen { get; set; }
        public string InstanciadoPor { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaTermino { get; set; }

        public ICollection<Tarea> Tareas { get; set; }
    }
}
