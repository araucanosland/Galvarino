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


    public class ExpedientesReparo
    {
        public string folioCredito { get; set; }
        public string valijaValorada { get; set; }
    }


    public class ColeccionEnvioNotariaFormHelper
    {
        public int Notaria { get; set; }
        public IEnumerable<EnvioNotariaFormHelper> Expedientes { get; set; }
    }

    public class ExpedienteGenerico{
        public string FolioCredito { get; set; }    
        public IEnumerable<string> DocumentosPistoleados { get; set; }
        public bool Faltante { get; set; }
        public string Marca { get; set; }
    }

    public class ColeccionExpedientesGenerica{

        public string CodOficina { get; set; }
        public int CodNotaria { get; set; }
        public IEnumerable<ExpedienteGenerico> ExpedientesGenericos { get; set; }
    }
}