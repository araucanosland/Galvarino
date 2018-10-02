using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Security
{
    public class Organizacion
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public TipoOrganizacion TipoOrganizacion { get; set; }
        public Organizacion Padre { get; set; }
        public ICollection<Organizacion> Hijos { get; set; }
        public ICollection<Rol> Roles { get; set; }
    }
}
