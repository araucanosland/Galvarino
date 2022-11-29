
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Security
{
    public static class CustomClaimTypes
    {
        public const string TipoAcceso = "http://galvarino.laaraucana.cl/identidad/claims/dominio/acceso"; 
        public const string UsuarioNombres = "http://galvarino.laaraucana.cl/identidad/claims/dominio/usuario/nombres"; 
        public const string OficinaCodigo = "http://galvarino.laaraucana.cl/identidad/claims/dominio/usuario/oficina/cod"; 
        public const string OficinaDescripcion = "http://galvarino.laaraucana.cl/identidad/claims/dominio/usuario/oficina/desc"; 
        public const string UsuarioCorreo = "http://galvarino.laaraucana.cl/identidad/claims/dominio/usuario/correo"; 
        public const string EsOficinaRM = "http://galvarino.laaraucana.cl/identidad/claims/dominio/usuario/oficina/rm";
        public const string Rut = "http://galvarino.laaraucana.cl/identidad/claims/dominio/usuario/rut";

    }
}
