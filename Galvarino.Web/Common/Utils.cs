using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;
using System;

namespace Galvarino.Web.Common
{
    public class Utils
    {

        public string QuitaAcento(string input)
        {

            string textoOriginal = input;
            string textoNormalizado = textoOriginal.Normalize(NormalizationForm.FormD);
            Regex reg = new Regex("[^a-zA-Z0-9 ]");
            string textoSinAcentos = reg.Replace(textoNormalizado, "");
            return textoSinAcentos;

        }


        public string GeneraTicket(string IdProceso)
        {
            DateTime now = DateTime.Now;
            return now.Year.ToString() + now.Month.ToString().PadLeft(2, '0') + now.Day.ToString().PadLeft(2, '0') + IdProceso.PadLeft(2, '0') + (now.Hour.ToString() + now.Minute.ToString() + now.Second.ToString() + now.Millisecond.ToString()).PadLeft(10, '0');
        }



    }
}
