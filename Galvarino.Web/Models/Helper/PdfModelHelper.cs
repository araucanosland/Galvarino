using System;
using System.Collections;
using System.Collections.Generic;
using Galvarino.Web.Models.Application;
using Galvarino.Web.Models.Workflow;

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
        public ICollection<Variable> Variables { get; set; }
    }

    public class ReporteValija
    {
        public string folioValija { get; set; }
        public string oficina { get; set; }
        public int cantidadExpedientes { get; set; }
        public DateTime? fechaPistoleo { get; set; }
    }
}