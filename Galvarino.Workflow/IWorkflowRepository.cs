using Galvarino.Workflow.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Galvarino.Workflow
{
    public interface IWorkflowRepository
    {

        Solicitud GenerarSolicitud(string nombreInternoProceso, string identificacionUsuario, string resumen);

        Solicitud GenerarSolicitud(string nombreInternoProceso, string identificacionUsuario, string resumen, Dictionary<string, string> variables);

        void ActivarTarea(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket, string identificacionUsuario);

        void ActivarTareaAsync(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket, string identificacionUsuario);

        void CompletarTarea(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket, string identificacionUsuario);

        void AbortarSolicitud(string NumeroTicket);

        Task AbortarSolicitudAsync(string NumeroTicket);

        void SetVariable(string key, string value, string numeroTicket);

        string GetVariableValue(string key, string numeroTicket);

        string QuienCerroEtapa(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket);

        IEnumerable<Tarea> TareasActivasUsuario(string nombreInternoProceso, string identificacionUsuario);

        Tarea ObtenerTareaByTicket(string nombreInternoProceso, string numeroTicket);
    }
}
