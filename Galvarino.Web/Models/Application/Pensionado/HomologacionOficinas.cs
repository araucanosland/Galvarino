using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Application.Pensionado
{
    public class HomologacionOficinas
    {
        [Key]
        public int Id { get; set; }
        [Column(TypeName = "varchar(20)")]
        public string  IdSucursalActividad { get; set; }
        [Column(TypeName = "int")]
        public int IdOficina { get; set; }
        [Column(TypeName = "varchar(100)")]
        public string Codificacion { get; set; }
        [Column(TypeName = "varchar(100)")] 
        public string Nombre { get; set; }

    }
}
