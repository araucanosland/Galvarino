using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.DocumentManagement.Model
{
    public class Region
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int Secuencia { get; set; }
        public ICollection<Comuna> Comunas { get; set; }
    }
}
