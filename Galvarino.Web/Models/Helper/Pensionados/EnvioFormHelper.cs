using System;
using System.Collections;
using System.Collections.Generic;

namespace Galvarino.Web.Models.Helper.Pensionado
{
    public class EnvioFormHelper{
        public string Folio { get; set; }
        public int Reparo { get; set; }
       

        public EnvioFormHelper()
        {
            Folio = string.Empty;
            Reparo = 0;
        }
    }


    public class ExpedientesReparo
    {
        public string Folio { get; set; }
        public string valijaValorada { get; set; }
    }


    public class ColeccionEnvioNotariaFormHelper
    {
        public int Notaria { get; set; }
        public IEnumerable<EnvioFormHelper> Expedientes { get; set; }
    }

    public class ExpedienteGenerico{
        public string Folio { get; set; }    
        public IEnumerable<string> DocumentosPistoleados { get; set; }
        public bool Faltante { get; set; }
        public string Marca { get; set; }
    }

    public class ColeccionExpedientesGenerica{

        public string CodOficina { get; set; }
        public int CodNotaria { get; set; }
        public IEnumerable<ExpedienteGenerico> ExpedientesGenericos { get; set; }
    }

    public class ColeccionDespachoPartes {
        public string Folio { get; set; }
        public string Rut { get; set; }
        public string Nombre { get; set; }
        public string Motivo { get; set; }
        public string Tipo { get; set; }
        public string Fecha { get; set; }

    }
}