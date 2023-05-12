using System;
using System.ComponentModel.DataAnnotations;

namespace Galvarino.Web.Models.Application.Pensionado
{
    public class Procesos
    {
        [Key]
        public int Id { get; set; }
        public Boolean Activo { get; set; }
        public string ClaseGeneraTickets { get; set; }
        public string MetodoGeneraTickets { get; set; }
        public string NamespaceGeneraTickets { get; set; }
        public string Nombre { get; set; }
        public string NombreInterno { get; set; }
  
    }
}
