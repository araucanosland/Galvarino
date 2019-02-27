using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.DocumentManagement.Model
{
    public class ValijaValorada
    {
        public int Id { get; set; }
        public DateTime FechaEnvio { get; set; }
        public Oficina Oficina { get; set; }
        public ICollection<ExpedienteCredito> Expedientes { get; set; }
        public string CodigoSeguimiento { get; set; }
        public string MarcaAvance { get; set; }
    }
}
