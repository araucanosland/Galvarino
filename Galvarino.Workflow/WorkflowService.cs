using Galvarino.Workflow.Infrastructure;
using Galvarino.Workflow.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Galvarino.Workflow
{
    public class WorkflowService : IWorkflowService
    {
        private readonly WorkflowRepository _repository;

        
        public WorkflowService(string connectionString)
        {
            DbContextOptionsBuilder<WorkflowDbContext> options = new DbContextOptionsBuilder<WorkflowDbContext>();
            options.UseSqlServer(connectionString);
            _repository = new WorkflowRepository(new WorkflowDbContext(options.Options));
        }

        public void Abortar()
        {
            throw new NotImplementedException();
        }

        public void AsignarVariable(string clave, string valor, string numeroTicket)
        {
            _repository.SetVariable(clave, valor, numeroTicket);
        }

        public void Avanzar(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket, string identificacionUsuario)
        {
            _repository.CompletarTarea(nombreInternoProceso, nombreInternoEtapa, numeroTicket, identificacionUsuario);
        }

        public async Task AvanzarRango(string nombreInternoProceso, string nombreInternoEtapa, IEnumerable<string> numeroTicket, string identificacionUsuario)
        {
            await Task.Run(() => {
                foreach (var item in numeroTicket)
                {
                    _repository.CompletarTarea(nombreInternoProceso, nombreInternoEtapa, item, identificacionUsuario);
                }
            });
        }

        public Solicitud Instanciar(string nombreProceso, string identificacionUsuario, string resumenInstancia)
        {
            return _repository.GenerarSolicitud(nombreProceso, identificacionUsuario, resumenInstancia);
        }

        public Solicitud Instanciar(string nombreProceso, string identificacionUsuario, string resumenInstancia, Dictionary<string, string> variables)
        {
            return _repository.GenerarSolicitud(nombreProceso, identificacionUsuario, resumenInstancia, variables);
        }

        public string ObtenerVariable(string clave, string numeroTicket)
        {
            return _repository.GetVariableValue(clave, numeroTicket);
        }

        public Tarea OntenerTareaActual(string nombreInternoProceso, string numeroTicket)
        {
            return _repository.ObtenerTareaByTicket(nombreInternoProceso, numeroTicket);
        }

        public string QuienCerroEtapa(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket)
        {
            return _repository.QuienCerroEtapa(nombreInternoProceso, nombreInternoEtapa, numeroTicket);
        }

        public IEnumerable<Tarea> TareasUsuario(string nombreProceso, string identificacionUsuario)
        {
            return _repository.TareasActivasUsuario(nombreProceso, identificacionUsuario);
        }
    }
}
