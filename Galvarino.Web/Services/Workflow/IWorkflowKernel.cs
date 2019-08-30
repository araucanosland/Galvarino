using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Web.Models.Workflow;
using Galvarino.Web.Models.Security;

namespace Galvarino.Web.Services.Workflow
{
    public interface IWorkflowKernel
    {
        Solicitud GenerarSolicitud(Proceso proceso, Usuario usuario);

        Solicitud GenerarSolicitud(string nombreInternoProceso, string identificacionUsuario, string resumen);

        Solicitud GenerarSolicitud(string nombreInternoProceso, string identificacionUsuario, string resumen, Dictionary<string,string> variables);

        void ActivarTarea(Proceso proceso, Etapa etapa, Usuario usuario);

        void ActivarTarea(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket, string identificacionUsuario);
        void ActivarTareaMulti(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket, string identificacionUsuario);

        void ActivarTareaAsync(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket, string identificacionUsuario);

        void CompletarTarea(Tarea tarea, Usuario usuario);

        void CompletarTarea(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket, string identificacionUsuario);

        void CompletarTareaMulti(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket, string identificacionUsuario);

        void CommitAvance();

        void AbortarSolicitud(Solicitud solicitud);

        void AbortarSolicitud(string NumeroTicket);

        Task AbortarSolicitudAsync(string NumeroTicket);

        void SetVariable(string key, string value, string numeroTicket);

        string GetVariableValue(string key, string numeroTicket);

        Usuario QuienCerroEtapa(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket);

        IEnumerable<Tarea> TareasActivasUsuario(string nombreInternoProceso, string identificacionUsuario);

        Tarea ObtenerTareaByTicket(string nombreInternoProceso, string numeroTicket);

        void ForzarAvance(string nombreInternoProceso, string nombreInternoEtapaDestino, IEnumerable<string> numeroTicket, string identificacionUsuario);
    }
}
