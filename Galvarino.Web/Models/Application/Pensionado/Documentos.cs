using System.ComponentModel.DataAnnotations;

namespace Galvarino.Web.Models.Application.Pensionado
{
    public class Documentos
    {
        [Key]
        public int Id { get; set; }
        public string Resumen { get; set; }
        public string Codificacion { get; set; }
        public ConfiguracionDocumentos ConfiguracionDocumento { get; set; }
        public int ExpedienteId { get; set; }

    }
}
