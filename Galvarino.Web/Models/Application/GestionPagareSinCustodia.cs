using System;
using System.ComponentModel.DataAnnotations;

namespace Galvarino.Web.Models.Application
{
    public class GestionPagareSinCustodia
    {

        [MaxLength(40)]
        public string Id { get; set; }
        
        public PagareSinCustodia PagareSinCustodia { get; set; }

        public DateTime FechaGestion { get; set; }

        public string Estado { get; set; }

        [MaxLength(500)]
        public string Resumen { get; set; }

        [MaxLength(50)]
        public string EjecutadoPor { get; set; }
    }
}