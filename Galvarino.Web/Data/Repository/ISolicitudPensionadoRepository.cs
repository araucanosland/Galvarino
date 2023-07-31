using System.Collections.Generic;
using Galvarino.Web.Models.Application;
using System;
using System.Data;
using System.Data.SqlClient;
using Galvarino.Web.Models.Helper.Pensionado;
using Galvarino.Web.Models.Application.Pensionado;

namespace Galvarino.Web.Data.Repository
{
    public interface ISolicitudPensionadoRepository
    {

        IEnumerable<SolicitudPensionados> ListarSolicitudes(string etapaIn = "");

        ExpendientesBuscado ObtenerExpedientes(string folio, string etapaSolicitud = "", string oficinaUsuario = "");

        string AvanzarRango(string nombreInternoProceso, string nombreInternoEtapa, IEnumerable<string> numeroTicket, string identificacionUsuario);

        ValijasValoradas DespachoOfPartes(string nombreInternoProceso, string nombreInternoEtapa, IEnumerable<ColeccionDespachoPartes> despachos, string identificacionUsuario,string user);
    }
}
