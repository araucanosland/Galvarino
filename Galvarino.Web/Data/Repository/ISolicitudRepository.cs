using Galvarino.Web.Models.Application;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Galvarino.Web.Data.Repository
{
    public interface ISolicitudRepository
    {

        
        IEnumerable<SolicitudResult> listarSolicitudes(string[] roles, string rut, string[] oficinas, string[] etapas, string order = null, string fechaConsulta = "", string notaria = "");

        IEnumerable<dynamic> listarResumenInicial(string[] roles, string rut, string[] oficinas);

        IEnumerable<dynamic> listarResumenInicial2(string[] roles, string rut, string[] oficinas);

        IEnumerable<SolicitudlistarValijasEnviadas> listarValijasEnviadas(string marcaAvance);

        IEnumerable<SolicitudResult> listarSolicitudesNominaEspecial(string[] roles, string rut, string[] oficinas, string[] etapas, string order = null, string fechaConsulta = "", string notaria = "");

        IEnumerable<dynamic> listarOficinas(string EsRM);

        IEnumerable<SolicitudAnalisisMC> ListarAnalisisMC(string[] roles, string nombreInterno);

        IEnumerable<dynamic> ActualizarEtapa(string rut, int solicitudid, int idtarea, string folio, bool tienevalija);

        string MoverEtapa(string FolioCredito, string EtapaaMover, string Encargado,string usuario);

        IEnumerable<SolicitudAnalisisMCReparos> ListarReparos(string FolioCredito,string nombreInterno);

        IEnumerable<dynamic> ReporteGestion(DateTime fechaInicial, DateTime fechaFinal);

        IEnumerable<ReporteProgramado> ListaRegistroReporteProgramado();

        DataTable ObtenerDataReporte(DateTime fechaInicial, DateTime fechaFinal);
    }
}