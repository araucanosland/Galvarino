using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Workflow.Model
{
    public class Transito
    {
        public int Id { get; set; }
        public Etapa EtapaActaual { get; set; }
        public Etapa EtapaDestino { get; set; }

        public string NamespaceValidacion { get; set; }
        public string ClaseValidacion { get; set; }
        public string MetodoValidacion { get; set; }
    }
}
