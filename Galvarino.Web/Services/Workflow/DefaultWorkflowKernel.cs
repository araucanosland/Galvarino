using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Web.Models.Security;
using Galvarino.Web.Models.Workflow;
using Galvarino.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Reflection.Emit;
using Galvarino.Web.Services.Workflow.Assignment;

namespace Galvarino.Web.Services.Workflow
{
    public class DefaultWorkflowKernel : IWorkflowKernel
    {

        private readonly ApplicationDbContext _context;
        
        public DefaultWorkflowKernel(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public void AbortarSolicitud(Solicitud solicitud)
        {
            throw new NotImplementedException();
        }

        public void AbortarSolicitud(string NumeroTicket)
        {
            DateTime momentoCierre = DateTime.Now;
            
            Solicitud solicitud  = _context.Solicitudes.FirstOrDefault(s => s.NumeroTicket == NumeroTicket);
            solicitud.FechaTermino = momentoCierre;
            solicitud.Estado = EstadoSolicitud.Abortada;

            Tarea tarea = _context.Tareas.FirstOrDefault(x => x.Solicitud.NumeroTicket == NumeroTicket && x.Estado == EstadoTarea.Iniciada && x.FechaTerminoFinal == null);
            tarea.FechaTerminoFinal = momentoCierre;
            tarea.Estado = EstadoTarea.Abortada;

            _context.Entry(solicitud).State = EntityState.Modified;
            _context.Entry(tarea).State = EntityState.Modified;

            //_context.SaveChanges();

        }

        public async Task AbortarSolicitudAsync(string NumeroTicket)
        {
            DateTime momentoCierre = DateTime.Now;

            Solicitud solicitud = await _context.Solicitudes.FirstOrDefaultAsync(s => s.NumeroTicket == NumeroTicket);
            solicitud.FechaTermino = momentoCierre;
            solicitud.Estado = EstadoSolicitud.Abortada;

            Tarea tarea = await _context.Tareas.FirstOrDefaultAsync(x => x.Solicitud.NumeroTicket == NumeroTicket && x.Estado == EstadoTarea.Iniciada && x.FechaTerminoFinal == null);
            tarea.FechaTerminoFinal = momentoCierre;
            tarea.Estado = EstadoTarea.Abortada;

            _context.Entry(solicitud).State = EntityState.Modified;
            _context.Entry(tarea).State = EntityState.Modified;

            //return Task.CompletedTask;
            //return await _context.SaveChangesAsync();
            
        }

        public void ActivarTarea(Proceso proceso, Etapa etapa, Usuario usuario)
        {
            throw new NotImplementedException();
        }

        public void ActivarTarea(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket, string identificacionUsuario)
        {
            //Primero obtengo el proceso
            Proceso proceso = _context.Procesos.Include(d => d.Etapas).Include(d=>d.Solicitudes).FirstOrDefault(x => x.NombreInterno == nombreInternoProceso);

            //Segundo Obtengo la etapa
            Etapa etapa = proceso.Etapas.FirstOrDefault(x => x.NombreInterno == nombreInternoEtapa);
            
            //Tercero obtengo la solicitud
            Solicitud solicitud = proceso.Solicitudes.FirstOrDefault(d => d.NumeroTicket == numeroTicket);


            string usuarioAsignado = etapa.ValorUsuarioAsignado;
            if (etapa.TipoUsuarioAsignado == TipoUsuarioAsignado.Auto)
            {
                usuarioAsignado = new MotorAsignacion(_context, etapa.ValorUsuarioAsignado, numeroTicket).GetResult();
            }
            else if(etapa.TipoUsuarioAsignado == TipoUsuarioAsignado.Variable)
            {
                usuarioAsignado = GetVariableValue(etapa.ValorUsuarioAsignado, numeroTicket);
            }


            Tarea tarea = new Tarea
            {
                Etapa = etapa,
                Estado = EstadoTarea.Activada,
                FechaInicio = DateTime.Now,
                Solicitud = solicitud,
                AsignadoA = usuarioAsignado
            };

            if (etapa.UnidadNegocioAsignar != null)
            {
                tarea.UnidadNegocioAsignada = GetVariableValue(etapa.UnidadNegocioAsignar, numeroTicket);
            }
            
            _context.Tareas.Add(tarea);
            _context.SaveChanges();

            if(etapa.TipoUsuarioAsignado == TipoUsuarioAsignado.Boot)
            {
                CompletarTarea(nombreInternoProceso, nombreInternoEtapa, numeroTicket, identificacionUsuario);
            }
        }

        public async void ActivarTareaAsync(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket, string identificacionUsuario)
        {
            //Primero obtengo el proceso
            Proceso proceso = await _context.Procesos.FirstOrDefaultAsync(x => x.NombreInterno == nombreInternoProceso);

            //Segundo Obtengo la etapa
            Etapa etapa = proceso.Etapas.FirstOrDefault(x => x.NombreInterno == nombreInternoEtapa);

            //Tercero obtengo la solicitud
            Solicitud solicitud = proceso.Solicitudes.FirstOrDefault(d => d.NumeroTicket == numeroTicket);

            Tarea tarea = new Tarea
            {
                Etapa = etapa,
                Estado = EstadoTarea.Activada,
                FechaInicio = DateTime.Now,
                Solicitud = solicitud,
                AsignadoA = etapa.ValorUsuarioAsignado
            };

            _context.Tareas.Add(tarea);
            await _context.SaveChangesAsync();
        }

        public void CompletarTarea(Tarea tarea, Usuario usuario)
        {
            throw new NotImplementedException();
        }

        public void CompletarTarea(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket, string identificacionUsuario)
        {
            Proceso proceso = _context.Procesos.FirstOrDefault(p => p.NombreInterno == nombreInternoProceso);
            Solicitud solicitud = _context.Solicitudes.Include(x=>x.Tareas).ThenInclude(t=>t.Etapa).FirstOrDefault(c => c.NumeroTicket.Equals(numeroTicket));
            Tarea tareaActual = solicitud.Tareas.FirstOrDefault(d => d.Etapa.NombreInterno.Equals(nombreInternoEtapa) && d.FechaTerminoFinal == null && d.Estado == EstadoTarea.Activada);

            tareaActual.EjecutadoPor = identificacionUsuario;
            tareaActual.FechaTerminoFinal = DateTime.Now;
            tareaActual.Estado = EstadoTarea.Finalizada;

            _context.Entry(tareaActual).State = EntityState.Modified;

            /*Cuando se instancia la etapa final esta cierra la solicitud */
            if(tareaActual.Etapa.TipoEtapa == TipoEtapa.Fin)
            {
                solicitud.Estado = EstadoSolicitud.Finalizada;
                solicitud.FechaTermino = DateTime.Now;
                _context.Solicitudes.Update(solicitud);
            }
            _context.SaveChanges();

            ICollection<Transito> transiciones = _context.Transiciones.Include(d => d.EtapaActaual).Include(d=>d.EtapaDestino).Where(d => d.EtapaActaual.NombreInterno == nombreInternoEtapa).ToList();
            foreach (Transito transicion in transiciones)
            {
                bool estadoAvance = EjecutvaValidacion(transicion.NamespaceValidacion, transicion.ClaseValidacion, numeroTicket);
                if(estadoAvance)
                {
                    this.ActivarTarea(nombreInternoProceso, transicion.EtapaDestino.NombreInterno, numeroTicket, identificacionUsuario);
                }
            }

           
        }

        public Solicitud GenerarSolicitud(Proceso proceso, Usuario usuario)
        {
            throw new NotImplementedException();
        }

        public Solicitud GenerarSolicitud(string nombreInternoProceso, string identificacionUsuario, string resumen)
        {
            Proceso proceso = _context.Procesos.Include(e => e.Etapas).FirstOrDefault(p => p.NombreInterno == nombreInternoProceso);
            string ticket = GeneraTicket(proceso.Id.ToString());
            Solicitud solicitud = new Solicitud
            {
                FechaInicio = DateTime.Now,
                Estado = EstadoSolicitud.Iniciada,
                InstanciadoPor = identificacionUsuario,
                NumeroTicket = ticket,
                Proceso = proceso,
                Resumen = resumen
            };

            _context.Solicitudes.Add(solicitud);
            _context.SaveChanges();

            var etapaInicial = proceso.Etapas.FirstOrDefault(x => x.TipoEtapa == TipoEtapa.Inicio && x.Secuencia == proceso.Etapas.Min(d => d.Secuencia));
            if (etapaInicial.TipoUsuarioAsignado == TipoUsuarioAsignado.Boot)
                identificacionUsuario = "wfboot";

            ActivarTarea(nombreInternoProceso, etapaInicial.NombreInterno, ticket, identificacionUsuario);

            return solicitud;
        }

        public Solicitud GenerarSolicitud(string nombreInternoProceso, string identificacionUsuario, string resumen, Dictionary<string, string> variables)
        {
            Proceso proceso = _context.Procesos.Include(e => e.Etapas).FirstOrDefault(p => p.NombreInterno == nombreInternoProceso);
            string ticket = GeneraTicket(proceso.Id.ToString());
            Solicitud solicitud = new Solicitud
            {
                FechaInicio = DateTime.Now,
                Estado = EstadoSolicitud.Iniciada,
                InstanciadoPor = identificacionUsuario,
                NumeroTicket = ticket,
                Proceso = proceso,
                Resumen = resumen
            };

            _context.Solicitudes.Add(solicitud);

            foreach(var varib in variables)
            {
                Variable var = new Variable
                {
                    NumeroTicket = ticket,
                    Clave = varib.Key,
                    Valor = varib.Value,
                    Tipo = "object"
                };
                _context.Variables.Add(var);
            }

            _context.SaveChanges();

            var etapaInicial = proceso.Etapas.FirstOrDefault(x => x.TipoEtapa == TipoEtapa.Inicio && x.Secuencia == proceso.Etapas.Min(d => d.Secuencia));
            if (etapaInicial.TipoUsuarioAsignado == TipoUsuarioAsignado.Boot)
                identificacionUsuario = "wfboot";

            ActivarTarea(nombreInternoProceso, etapaInicial.NombreInterno, ticket, identificacionUsuario);

            return solicitud;
        }

        private string GeneraTicket(string IdProceso)
        {
            DateTime now = DateTime.Now;
            return now.Year.ToString() + now.Month.ToString().PadLeft(2, '0') + now.Day.ToString().PadLeft(2, '0') + IdProceso.PadLeft(2, '0') + (now.Hour.ToString() + now.Minute.ToString() + now.Second.ToString() + now.Millisecond.ToString()).PadLeft(10, '0');
        }

        private bool EjecutvaValidacion(string elnamespace, string laclase, string numeroTicket)
        {

            object[] losparametros =
            {
                _context,
                numeroTicket
            };
            Type type = typeof(IWorkflowTransitionValidation);
            MethodInfo method = type.GetMethod("Validar");
            Type implementacion = Type.GetType(elnamespace + "." + laclase);
            IWorkflowTransitionValidation instancia = (IWorkflowTransitionValidation)Activator.CreateInstance(implementacion, losparametros);
            return (bool)method.Invoke(instancia, null);
        }
        
        public void SetVariable(string key, string value, string numeroTicket)
        {
            if(_context.Variables.Any(s=>s.Clave == key && s.NumeroTicket == numeroTicket))
            {
                Variable variable = _context.Variables.FirstOrDefault(s => s.Clave == key && s.NumeroTicket == numeroTicket);
                variable.Valor = value;
                _context.Entry(variable).State = EntityState.Modified;
                _context.SaveChanges();
            }
            else
            {
                Variable variable = new Variable
                {
                    Clave = key,
                    Valor = value,
                    NumeroTicket = numeroTicket,
                    Tipo = "string"
                };
                _context.Variables.Add(variable);
                _context.SaveChanges();
            }
        }

        public string GetVariableValue(string key, string numeroTicket)
        {
            Variable variable = _context.Variables.FirstOrDefault(d => d.Clave == key && d.NumeroTicket == numeroTicket);
            return variable != null ? variable.Valor : "";
        }

        public IEnumerable<Tarea> TareasActivasUsuario(string nombreInternoProceso, string identificacionUsuario)
        {
            return _context.Tareas.Include(t => t.Solicitud).ThenInclude(x=>x.Proceso).Where(t => t.AsignadoA == identificacionUsuario && t.Solicitud.Proceso.NombreInterno == nombreInternoProceso && t.Estado == EstadoTarea.Activada && t.FechaTerminoFinal == null).ToList();
        }

        public Usuario QuienCerroEtapa(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket)
        {
            var laSol = _context.Solicitudes
                            .Include(s => s.Tareas).ThenInclude(t => t.Etapa)
                            .FirstOrDefault(f => f.Proceso.NombreInterno == nombreInternoProceso && f.NumeroTicket == numeroTicket);
            var ss = laSol.Tareas.FirstOrDefault(x => x.Etapa.NombreInterno == nombreInternoEtapa && x.Estado == EstadoTarea.Finalizada && x.FechaTerminoFinal != null);
            return _context.Users.FirstOrDefault(us => us.Identificador == ss.EjecutadoPor);

        }

        public Tarea ObtenerTareaByTicket(string nombreInternoProceso, string numeroTicket)
        {
            return _context.Tareas
                            .Include(tar => tar.Etapa)
                            .Include(tar => tar.Solicitud)
                            .FirstOrDefault(d => d.Solicitud.NumeroTicket == numeroTicket && d.Estado == EstadoTarea.Activada);
        }
    }
}
