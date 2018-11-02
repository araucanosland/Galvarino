using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Helper
{
    public class Workflow
    {

    }


    public class EtapaHelper
    {
        public IEnumerable<TransicionHelper> destinos { get; set; }
        public int id { get; set; }
        public string nombre { get; set; }
        public int proceso { get; set; }
        public string secuencia { get; set; }
        public string tipoDuracion { get; set; }
        public string tipoDuracionRetardo { get; set; }
        public string tipoEtapa { get; set; }
        public string tipoUsuarioAsignado { get; set; }
        public string valorDuracion { get; set; }
        public string valorDuracionRetardo { get; set; }
        public string valorUsuarioAsignado { get; set; }
        public string enlace { get; set; }
    }

    public class TransicionHelper
    {
        public int id { get; set; }
        public int etapaDestino { get; set; }
        public string namespaceValidacion { get; set; }
        public string claseValidacion { get; set; }
        public string metodoValidacion { get; set; }
    }
}