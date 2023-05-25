using Galvarino.Web.Models.Workflow;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace Galvarino.Web.Models.Application.Pensionado
{
    public class Solicitudes
    {
        public int Id { get; set; }
        public string NumeroTicket { get; set; }
        public Procesos Proceso { get; set; }
        public string Estado { get; set; }
        public string Resumen { get; set; }
        public string InstanciadoPor { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaTermino { get; set; }

        //public ICollection<Tarea> Tareas { get; set; }

    }
    
}
