using System;
using System.Collections;
using System.Collections.Generic;

namespace Galvarino.Web.Models.Helper
{
    public class EnvioNotariaFormHelper{
        public string FolioCredito { get; set; }
        public bool Aprobado { get; set; }
        public string Comentario { get; set; }

        public EnvioNotariaFormHelper()
        {
            FolioCredito = string.Empty;
            Aprobado = false;
            Comentario = string.Empty;
        }
    }
}