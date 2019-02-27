using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Workflow.Model
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
