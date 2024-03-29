﻿using Galvarino.Web.Models.Application;
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

        public void Avanzar(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket, string identificacionUsuario)
        {
            _kernel.CompletarTarea(nombreInternoProceso, nombreInternoEtapa, numeroTicket, identificacionUsuario);
        }

        public Solicitud Instanciar(string nombreProceso, string identificacionUsuario, string resumenInstancia)
        {
            return _kernel.GenerarSolicitud(nombreProceso, identificacionUsuario, resumenInstancia);
        }

        public Solicitud InstanciarHistorico(string nombreProceso, string identificacionUsuario, string resumenInstancia, Dictionary<string, string> variables)
        {
            return _kernel.GenerarSolicitudHistorico(nombreProceso, identificacionUsuario, resumenInstancia, variables);


        }


        public Solicitud Instanciar(string nombreProceso, string identificacionUsuario, string resumenInstancia, Dictionary<string, string> variables)
        {
            return _kernel.GenerarSolicitud(nombreProceso, identificacionUsuario, resumenInstancia, variables);
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

        public async Task AvanzarRango(string nombreInternoProceso, string nombreInternoEtapa, IEnumerable<string> numeroTicket, string identificacionUsuario)
        {
            foreach (var item in numeroTicket)
            {
                _kernel.CompletarTareaMulti(nombreInternoProceso, nombreInternoEtapa, item, identificacionUsuario);
            }
            _kernel.CommitAvance();

            await Task.CompletedTask;
        }

        public async Task AvanzarRangoAnalisisMC(string nombreInternoProceso, string nombreInternoEtapa, IEnumerable<string> numeroTicket, string identificacionUsuario)
        {
            foreach (var item in numeroTicket)
            {
                _kernel.CompletarTareaMultiAnalisisMC(nombreInternoProceso, nombreInternoEtapa, item, identificacionUsuario);
            }
            _kernel.CommitAvance();

            await Task.CompletedTask;
        }

        public async Task AvanzarRangoHistorico(string nombreInternoProceso, string nombreInternoEtapa, IEnumerable<string> numeroTicket, string identificacionUsuario)
        {
            foreach (var item in numeroTicket)
            {
                _kernel.CompletarTareaMultiHistorico(nombreInternoProceso, nombreInternoEtapa, item, identificacionUsuario);
            }
            _kernel.CommitAvance();

            await Task.CompletedTask;
        }

        public Usuario QuienCerroEtapa(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket)
        {
            return _kernel.QuienCerroEtapa(nombreInternoProceso, nombreInternoEtapa, numeroTicket);
        }

        public Tarea OntenerTareaActual(string nombreInternoProceso, string numeroTicket)
        {
            return _kernel.ObtenerTareaByTicket(nombreInternoProceso, numeroTicket);
        }

        public void ForzarAvance(string nombreInternoProceso, string nombreInternoEtapaDestino, IEnumerable<string> numeroTicket, string identificacionUsuario)
        {
            _kernel.ForzarAvance(nombreInternoProceso, nombreInternoEtapaDestino, numeroTicket, identificacionUsuario);
        }

        public async Task generarValijaReparo(string nombreInternoProceso, string nombreInternoEtapaDestino, IEnumerable<DevolucionReparos> folioReparos, string identificacionUsuario)
        {

          await  _kernel.generarValijaReparo(nombreInternoProceso, nombreInternoEtapaDestino, folioReparos, identificacionUsuario);



        }


    }
}
