using System;

namespace Galvarino.Web.Models.Application.Pensionado
{
    public class ValijasValoradas
    {
        public int Id { get; set; }
        public DateTime FechaEnvio { get; set; }
        public HomologacionOficinas HomologacionOficinas { get; set; }
        public String CodigoSeguimiento { get; set; }
        public String MarcaAvance { get; set; } 
    }
}
