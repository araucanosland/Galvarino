using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Application
{
    public class CajaValorada
    {
        public int Id { get; set; }
        public DateTime FechaEnvio { get; set; }
        public ICollection<ExpedienteCredito> Expedientes { get; set; }
        public string CodigoSeguimiento { get; set; }
        public string MarcaAvance { get; set; }
        public string Usuario { get; set; }
    }
}
