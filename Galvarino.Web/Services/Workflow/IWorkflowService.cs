using Galvarino.Web.Models.Security;
using Galvarino.Web.Models.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Services.Workflow
{
    public interface IWorkflowService
    {
        Solicitud Instanciar(string nombreProceso, string identificacionUsuario, string resumenInstancia);

        Solicitud Instanciar(string nombreProceso, string identificacionUsuario, string resumenInstancia, Dictionary<string, string> variables);

        void Avanzar(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket, string identificacionUsuario);

        IEnumerable<Tarea> TareasUsuario(string nombreProceso, string identificacionUsuario);

        Task AvanzarRango(string nombreInternoProceso, string nombreInternoEtapa, IEnumerable<string> numeroTicket, string identificacionUsuario);

        void Abortar();

        void AsignarVariable(string clave, string valor, string numeroTicket);

        string ObtenerVariable(string clave, string numeroTicket);

        Usuario QuienCerroEtapa(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket);
    }
}
