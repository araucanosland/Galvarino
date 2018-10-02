using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Application
{
    public class Oficina
    {
        public int Id { get; set; }
        public string Codificacion { get; set; }
        public string Nombre { get; set; }
        public Comuna Comuna { get; set; }
        public Oficina OficinaProceso { get; set; }
        public ICollection<PackNotaria> PacksNotaria { get; set; }
    }
}
