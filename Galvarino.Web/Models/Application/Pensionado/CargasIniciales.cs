using System;
using System.ComponentModel.DataAnnotations;

namespace Galvarino.Web.Models.Application.Pensionado
{
    public class CargasIniciales
    {
        [Key]
        public int Id { get; set; }
        public CargasInicialesEstado CargaInicialEstado { get; set; }
        public DateTime FechaCarga { get; set; }
        public DateTime FechaProceso { get; set; }
        public string Folio { get; set; }
        public string Estado { get; set; }
        public string RutPensionado { get; set; }
        public string DvPensionado { get; set; }
        public string NombrePensionado { get; set; }
        public Tipo Tipo { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public DateTime FechaEfectiva { get; set; }
        public Sucursal Sucursal { get; set; }
        public string Forma { get; set; }
        public string TipoMovimiento { get; set; }


    }
}
