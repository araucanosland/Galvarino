using System.Collections.Generic;
using Galvarino.Web.Models.Application;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Galvarino.Web.Data.Repository
{
    public interface ISolicitudPensionadoRepository
    {

        IEnumerable<SolicitudPensionados> ListarSolicitudes(string etapaIn = "");
        
    }
}
