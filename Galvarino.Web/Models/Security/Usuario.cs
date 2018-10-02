using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Web.Models.Application;


namespace Galvarino.Web.Models.Security
{
    public class Usuario : IdentityUser
    {
        public string Identificador { get; set; }
        public TipoEstadoCuenta EstadoCuenta { get; set; }
        public string TokenRecuerdaAcceso { get; set; }
        public bool Eliminado { get; set; }
        public string Nombres { get; set; }
        public Oficina Oficina { get; set; }
    }
}
