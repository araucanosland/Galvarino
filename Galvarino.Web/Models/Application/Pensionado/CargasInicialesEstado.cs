using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Application.Pensionado
{
    public class CargasInicialesEstado
    {
        public int Id  { get; set; }
        public DateTime FechaCarga { get; set; }
        public string NombreArchivoCarga { get; set; }
        public string Estado { get; set; }
        public int Procesado { get; set; }

    }
}
