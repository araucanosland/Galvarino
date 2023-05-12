using System.ComponentModel.DataAnnotations;

namespace Galvarino.Web.Models.Application.Pensionado
{
    public class ConfiguracionDocumentos
    {
        [Key]
        public int Id { get; set; }
        public int TipoDocumento { get; set; }
        public int TipoExpediente { get; set; }
        public int TipoPensionado { get; set; }
        public string Codificacion { get; set; }
        public string NombreDocumento { get; set; }
        public string NombreInterno { get; set; }
        public int Activo { get; set; }


    }
}
