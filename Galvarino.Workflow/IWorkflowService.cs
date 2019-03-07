using Galvarino.Workflow.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Galvarino.Workflow
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

        string QuienCerroEtapa(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket);

        Tarea OntenerTareaActual(string nombreInternoProceso, string numeroTicket);
    }
}
