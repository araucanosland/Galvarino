using System;
using System.Collections;
using System.Collections.Generic;
using Galvarino.Web.Models.Application.Pensionado;
using Galvarino.Web.Models.Workflow;

namespace Galvarino.Web.Models.Helper.Pensionado
{
    public class PdfModelHelper
    {
        public string FechaImpresion { get; set; }
        public string CodigoSeguimiento { get; set; }
        public string MarcaDocto { get; set; }
        public string Reparos { get; set; }
        public ValijasValoradas ValijaValorada { get; set; }
        public ICollection<PdfParametros> Expedientes { get; set; }
        public ICollection<Documentos> Documentos { get; set; }
    }

    public class ReporteValija
    {
        public string folioValija { get; set; }
        public string oficina { get; set; }
        public int cantidadExpedientes { get; set; }
        public DateTime? fechaPistoleo { get; set; }
    }

    public class PdfParametros
    {
        public int Id { get; set; }
        public string Folio { get; set; }
        public string RutCliente { get; set; }
        public string TipoDescripcion { get; set; }
        public string Motivo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}