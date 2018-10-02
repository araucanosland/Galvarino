using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Security
{
    public class InfoRol
    {
        public string Nombre { get; set; }
        public int OrganizacionId { get; set; }
        public string PadreId { get; set; }
    }
}
