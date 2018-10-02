using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Workflow
{
    public class Notificacion
    {
        public int Id { get; set; }
        public ICollection<string> Destinatarios { get; set; }
        public string Titulo { get; set; }
        public string Resumen { get; set; }
        public string Cuerpo { get; set; }
    }
}
