using System;
using System.Collections;
using System.Collections.Generic;
using Galvarino.Web.Models.Application;

namespace Galvarino.Web.Models.Helper
{
    public class PdfModelHelper
    {
        public string FechaImpresion { get; set; }
        public string CodigoSeguimiento { get; set; }
        public string MarcaDocto { get; set; }    
        public string Reparos { get; set; }    
        public PackNotaria PackNotaria { get; set; }
        public ValijaValorada ValijaValorada { get; set; }
        public CajaValorada CajaValorada { get; set; }
        public ValijaOficina ValijaOficina { get; set; }
    }
}