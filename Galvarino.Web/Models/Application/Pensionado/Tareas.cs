using Galvarino.Web.Models.Workflow;
using System;
using System.ComponentModel.DataAnnotations;

namespace Galvarino.Web.Models.Application.Pensionado
{
    public class Tareas
    {
        [Key]
        public int Id { get; set; }
        public Solicitudes Solicitudes { get; set; }
        public Etapas Etapas { get; set; }
        public string AsignadoA { get; set; }
        public string ReasignadoA { get; set; }
        public string EjecutadoPor { get; set; }
        public string Estado { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaTerminoEstimada { get; set; }
        public DateTime FechaTerminoFinal { get; set; }

    }
}
