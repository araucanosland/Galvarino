using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Application
{
    public class AlmacenajeComercial
    {
        public Guid Id { get; set; }
        public DateTime Fecha { get; set; }
        public ICollection<ExpedienteCredito> Expedientes { get; set; }
        public string CodigoSeguimiento { get; set; }
        public string RutEjecutivo { get; set; }
        public string CodigoOficina { get; set; }
        //public string Resumen { get; set; }

        public AlmacenajeComercial(){
            Expedientes = new List<ExpedienteCredito>();
        }

    }
}
