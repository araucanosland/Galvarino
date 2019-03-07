using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.DocumentManagement.Model
{
    public class ValijaOficina
    {
        public int Id { get; set; }
        public DateTime FechaEnvio { get; set; }
        public Oficina OficinaEnvio { get; set; }
        public Oficina OficinaDestino { get; set; }
        public ICollection<ExpedienteCredito> Expedientes { get; set; }
        public string CodigoSeguimiento { get; set; }
        public string MarcaAvance { get; set; }
    }
}
