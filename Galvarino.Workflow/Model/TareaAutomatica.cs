using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Workflow.Model
{
    public class TareaAutomatica
    {
        public int Id { get; set; }
        public Etapa Etapa { get; set; }
        public string Descripcion { get; set; }
        public TipoEventoEtapa EventoDisparador { get; set; }
        public string Namespace { get; set; }
        public string Clase { get; set; }
        public string Metodo { get; set; }
        public int Secuencia { get; set; }
    }
}
