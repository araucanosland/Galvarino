using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Workflow
{
    public class Etapa
    {
        public int Id { get; set; }
        public Proceso Proceso { get; set; }
        public TipoEtapa TipoEtapa { get; set; }
        public string Nombre { get; set; }
        public string NombreInterno { get; set; }
        public TipoUsuarioAsignado TipoUsuarioAsignado { get; set; }
        public string ValorUsuarioAsignado { get; set; }
        public TipoDuracion TipoDuracion { get; set; }
        public string ValorDuracion { get; set; }
        public TipoDuracion TipoDuracionRetardo { get; set; }
        public string ValorDuracionRetardo { get; set; }
        public int Secuencia { get; set; }
        public string Link { get; set; }
        public string UnidadNegocioAsignar { get; set; }
        public ICollection<TareaAutomatica> TareasAutomaticas { get; set; }
        public ICollection<Transito> Destinos { get; set; }
        public ICollection<Transito> Actuales { get; set; }
        public ICollection<Tarea> Tareas { get; set; }
    }
}
