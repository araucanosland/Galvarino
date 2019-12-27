using System.Collections.Generic;

namespace Galvarino.Web.Data.Repository
{
    public interface ISolicitudRepository
    {
        IEnumerable<SolicitudResult> listarSolicitudes(string[] roles, string rut, string[] oficinas, string[] etapas, string order = null, string fechaConsulta = "", string notaria = "");

        IEnumerable<dynamic> listarResumenInicial(string[] roles, string rut, string[] oficinas);

        IEnumerable<SolicitudResult> listarSolicitudesNominaEspecial(string[] roles, string rut, string[] oficinas, string[] etapas, string order = null, string fechaConsulta = "", string notaria = "");

        IEnumerable<SolicitudResult> listarSolicitudesSc(string[] roles, string rut, string[] oficinas, string[] etapas, string order = null, string fechaConsulta = "", string sucrusal = "");


        IEnumerable<dynamic> listarValijasEnviadas(string marcaAvance);




        IEnumerable<dynamic> listarValijasEnviadasSc(string marcaAvance);


        IEnumerable<dynamic> listarSegurosPorValijas(string skp,string rut);

        IEnumerable<dynamic> listarValijasEnviadasOficinaIngresoSc(string marcaAvance, string[] roles, string rut, string[] oficinas, string[] etapaIn);

        IEnumerable<dynamic> listarOficinas(string EsRM);

        IEnumerable<dynamic> listarSucursalespoEtapa();

    }
}