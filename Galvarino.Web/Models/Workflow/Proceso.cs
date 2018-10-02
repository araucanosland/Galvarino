using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Workflow
{
    public class Proceso
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string NombreInterno { get; set; }
        public string NamespaceGeneraTickets { get; set; }
        public string ClaseGeneraTickets { get; set; }
        public string MetodoGeneraTickets { get; set; }
        public bool Activo { get; set; }
        public ICollection<Etapa> Etapas { get; set; }
        public ICollection<Solicitud> Solicitudes { get; set; }
    }
}
