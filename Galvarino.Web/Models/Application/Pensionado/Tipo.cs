using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Application.Pensionado
{
    public class Tipo
    {
        [Key]
        public string Id { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string TipoDescripcion { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string Motivo { get; set; }

    }
}
