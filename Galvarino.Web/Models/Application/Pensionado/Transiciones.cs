using System.ComponentModel.DataAnnotations;

namespace Galvarino.Web.Models.Application.Pensionado
{
    public class Transiciones
    {
        [Key]
        public int Id { get; set; }
        public int EtapaActaualId { get; set; }
        public int EtapaDestinoId { get; set; }
        public string NamespaceValidacion { get; set; }
        public string ClaseValidacion { get; set; }
        public string MetodoValidacion { get; set; }


    }
}
