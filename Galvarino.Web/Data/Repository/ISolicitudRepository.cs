using System.Collections.Generic;

namespace Galvarino.Web.Data.Repository
{
    public interface ISolicitudRepository
    {
        IEnumerable<SolicitudResult> listarSolicitudes(string[] roles, string rut, string[] oficinas, string[] etapas, string order = null);

        IEnumerable<dynamic> listarResumenInicial(string[] roles, string rut, string[] oficinas); 
    }
}