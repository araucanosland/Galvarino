using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Security
{
    public class InfoUsuario
    {
        public string Nombres { get; set; }
        public string Identificacion { get; set; }
        public string Correo { get; set; }
        public string Llave { get; set; }
        public string CodificacionOficina { get; set; }
        public string Firma { get; set; }
        public string Rol { get; set; }
    }
}