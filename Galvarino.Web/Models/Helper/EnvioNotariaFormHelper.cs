using System;
using System.Collections;
using System.Collections.Generic;

namespace Galvarino.Web.Models.Helper
{
    public class EnvioNotariaFormHelper{
        public string FolioCredito { get; set; }
        public int Reparo { get; set; }

        public EnvioNotariaFormHelper()
        {
            FolioCredito = string.Empty;
            Reparo = 0;
        }
    }
}