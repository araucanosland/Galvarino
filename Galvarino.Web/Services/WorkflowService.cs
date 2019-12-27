using Galvarino.Web.Models.Security;
using Galvarino.Web.Models.Workflow;
using Galvarino.Web.Services.Workflow;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Galvarino.Web.Services
{
    public class WorkflowService : IWorkflowService
    {

        private readonly IWorkflowKernel _kernel;
        public WorkflowService(IWorkflowKernel kernel)
        {
            _kernel = kernel;
        }

        public void Abortar()
        {
            throw new NotImplementedException();
        }


        public Etapa OntenerTareaActualSc(string nombreInternoProceso, string FolioCredito)
        {
            return _kernel.ObtenerTareaByFolioSc(nombreInternoProceso, FolioCredito);
        }

        public void Avanzar(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket, string identificacionUsuario)
        {
            _kernel.CompletarTarea(nombreInternoProceso, nombreInternoEtapa, numeroTicket, identificacionUsuario);
        }
        public async Task AvanzarSetComplementario(string nombreInternoProceso, string nombreInternoEtapa, IEnumerable<string> numeroTicket, string identificacionUsuario)
        {
            foreach (var item in numeroTicket)
            {
                _kernel.CompletarTareaSetComplementario(nombreInternoProceso, nombreInternoEtapa, item, identificacionUsuario);
            }
            _kernel.CommitAvance();

            await Task.CompletedTask;
        }


        public async Task AvanzarRango(string nombreInternoProceso, string nombreInternoEtapa, IEnumerable<string> numeroTicket, string identificacionUsuario)
        {
            foreach (var item in numeroTicket)
            {
                _kernel.CompletarTareaMulti(nombreInternoProceso, nombreInternoEtapa, item, identificacionUsuario);
            }
            _kernel.CommitAvance();

            await Task.CompletedTask;
        }



        public Solicitud Instanciar(string nombreProceso, string identificacionUsuario, string resumenInstancia)
        {
            return _kernel.GenerarSolicitud(nombreProceso, identificacionUsuario, resumenInstancia);
        }

        public Solicitud Instanciar(string nombreProceso, string identificacionUsuario, string resumenInstancia, Dictionary<string, string> variables)
        {
            return _kernel.GenerarSolicitud(nombreProceso, identificacionUsuario, resumenInstancia, variables);
        }


        public Solicitud InstanciarSc(string nombreProceso, string identificacionUsuario, string resumenInstancia, Dictionary<string, string> variables)
        {
            return _kernel.GenerarSolicitudSc(nombreProceso, identificacionUsuario, resumenInstancia, variables);
        }

        public void AsignarVariable(string clave, string valor, string numeroTicket)
        {
            _kernel.SetVariable(clave, valor, numeroTicket);
        }

        public string ObtenerVariable(string clave, string numeroTicket)
        {
            return _kernel.GetVariableValue(clave, numeroTicket);
        }

        public IEnumerable<Tarea> TareasUsuario(string nombreProceso, string identificacionUsuario)
        {
            throw new NotImplementedException();
        }

     

        public Usuario QuienCerroEtapa(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket)
        {
            return _kernel.QuienCerroEtapa(nombreInternoProceso, nombreInternoEtapa, numeroTicket);
        }

        public Usuario QuienCerroEtapaSetComplementario(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket)
        {
            return _kernel.QuienCerroEtapaSetComplementario(nombreInternoProceso, nombreInternoEtapa, numeroTicket);
        }

        public Tarea OntenerTareaActual(string nombreInternoProceso, string numeroTicket)
        {
            return _kernel.ObtenerTareaByTicket(nombreInternoProceso, numeroTicket);
        }

        public void ForzarAvance(string nombreInternoProceso, string nombreInternoEtapaDestino, IEnumerable<string> numeroTicket, string identificacionUsuario)
        {
            _kernel.ForzarAvance(nombreInternoProceso, nombreInternoEtapaDestino, numeroTicket, identificacionUsuario);
        }
    }
}
