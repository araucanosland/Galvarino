﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Models.Application
{
    public class PackNotaria
    {
        public int Id { get; set; }
        public DateTime FechaEnvio { get; set; }
        public Notaria NotariaEnvio { get; set; }
        public Oficina Oficina { get; set; }
        public ICollection<ExpedienteCredito> Expedientes { get; set; }

    }
}
