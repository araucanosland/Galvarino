using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.DocumentManagement.Model
{
    public class Comuna
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public Region Region { get; set; }
    }
}
