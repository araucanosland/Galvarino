using System;
using System.ComponentModel.DataAnnotations;

namespace Galvarino.Web.Models.Application.Pensionado
{
    public class LogCargaInicial
    {
        [Key]
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Paso { get; set; }
        public string CampoReferencia { get; set; }
        public string Identificador { get; set; }
        public string CodigoEstado { get; set; }
        public string Cometario { get; set; }
        

    }
}
