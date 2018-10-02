using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Application
{
    public class Notaria
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public Comuna Comuna { get; set; }
    }
}
