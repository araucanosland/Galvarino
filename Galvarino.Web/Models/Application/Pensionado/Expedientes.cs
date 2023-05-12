using System;
using System.ComponentModel.DataAnnotations;

namespace Galvarino.Web.Models.Application.Pensionado
{
    public class Expedientes
    {
        [Key]
        public int Id { get; set; }
        public DateTime FechaCreacion { get; set; }
        public Pensionado Pensionado { get; set; }
        public int TipoExpediente { get; set; }
        public int ValijaValoradaId { get; set; }
        public int CajaValoradaId { get; set; }
        public int ValijaOficinaId { get; set; }
        public string IdSucursalActividad { get; set; }
    }
}
