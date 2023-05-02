using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;
using System;
using System.Globalization;

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

        public string formatearFecha(string dato,string formato)
        {

            switch (formato)
            {
                case "YYYYMMDD":
                    dato = dato.Substring(0, 4) + "/" + dato.Substring(4, 2) + "/" + dato.Substring(6, 2);
                    break;

                case "DDMMYYYY":
                    dato = dato.Substring(0, 2) + "/" + dato.Substring(2, 2) + "/" + dato.Substring(4, 4);
                    break;

                default:
                    Console.WriteLine($"error al convertir a formato {formato}.");
                    dato = null;
                    break;
            }
            return dato;
        }

    }
}
