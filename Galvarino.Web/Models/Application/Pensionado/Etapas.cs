using System.ComponentModel.DataAnnotations;

namespace Galvarino.Web.Models.Application.Pensionado
{
    public class Etapas
    {
        [Key]
        public int Id { get; set; }
        public string TipoEtapa { get; set; }
        public string Nombre { get; set; }
        public string NombreInterno { get; set; }
        public string TipoUsuarioAsignado { get; set; }
        public string ValorUsuarioAsignado { get; set; }
        public string TipoDuracion { get; set; }
        public string ValorDuracion { get; set; }
        public string TipoDuracionRetardo { get; set; }
        public string ValorDuracionRetardo { get; set; }
        public int Secuencia { get; set; }
        public string Link { get; set; }
        public string UnidadNegocioAsignar { get; set; }

    }
}
