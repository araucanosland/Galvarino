﻿using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;

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




      
    }
}