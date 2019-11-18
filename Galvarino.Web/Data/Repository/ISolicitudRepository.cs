using System.Collections.Generic;

namespace Galvarino.Web.Data.Repository
{
    public interface ISolicitudRepository
    {
        IEnumerable<SolicitudResult> listarSolicitudes(string[] roles, string rut, string[] oficinas, string[] etapas, string order = null, string fechaConsulta = "", string notaria = "");

        IEnumerable<dynamic> listarResumenInicial(string[] roles, string rut, string[] oficinas);

        IEnumerable<SolicitudResult> listarSolicitudesNominaEspecial(string[] roles, string rut, string[] oficinas, string[] etapas, string order = null, string fechaConsulta = "", string notaria = "");



        IEnumerable<dynamic> listarValijasEnviadas(string marcaAvance);

        IEnumerable<dynamic> listarOficinas(string EsRM);
    }
}