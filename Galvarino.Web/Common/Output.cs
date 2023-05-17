using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using System;

namespace Galvarino.Web.Common
{
    public class Output
    {
        public Output()
        : this(null, null, null, null, null)
        { }

        public Output(string mensaje, string codigo, string paso, string identificador, string campoReferencia)
        {
            Mensaje = mensaje;
            Codigo = codigo;
            Paso = paso;
            Identificador = identificador;  
            CampoReferencia = campoReferencia;
        }
        public string Mensaje { get;set; }
        public string Codigo { get; set; }
        public string Paso { get; set; }
        public string Identificador { get; set; }
        public string CampoReferencia { get; set; }

        public override string ToString() => $"({Mensaje},\r\n {Codigo},\r\n{Paso},\r\n{Identificador},\r\n{CampoReferencia} )";
    }

}
