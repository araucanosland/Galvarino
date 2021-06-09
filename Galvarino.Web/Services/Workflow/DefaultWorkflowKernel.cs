using Dapper;
using Galvarino.Web.Data;
using Galvarino.Web.Models.Exception;
using Galvarino.Web.Models.Security;
using Galvarino.Web.Models.Workflow;
using Galvarino.Web.Services.Workflow.Assignment;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Galvarino.Web.Services.Workflow
{
    public class DefaultWorkflowKernel : IWorkflowKernel
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        //private object _configuration;

        public DefaultWorkflowKernel(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public void AbortarSolicitud(Solicitud solicitud)
        {
            throw new NotImplementedException();
        }

        public void AbortarSolicitud(string NumeroTicket)
        {
            DateTime momentoCierre = DateTime.Now;

            Solicitud solicitud = _context.Solicitudes.FirstOrDefault(s => s.NumeroTicket == NumeroTicket);
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
            Proceso proceso = _context.Procesos.FirstOrDefault(x => x.NombreInterno == nombreInternoProceso && x.Id==1);

            //Segundo Obtengo la etapa
            Etapa etapa = _context.Etapas.Include(e => e.Proceso).FirstOrDefault(x => x.NombreInterno == nombreInternoEtapa && x.Proceso == proceso);

            //Tercero obtengo la solicitud
            Solicitud solicitud = _context.Solicitudes.FirstOrDefault(d => d.NumeroTicket == numeroTicket);


            string usuarioAsignado = etapa.ValorUsuarioAsignado;
            if (etapa.TipoUsuarioAsignado == TipoUsuarioAsignado.Auto)
            {
                usuarioAsignado = new MotorAsignacion(_context, etapa.ValorUsuarioAsignado, numeroTicket).GetResult();
            }
            else if (etapa.TipoUsuarioAsignado == TipoUsuarioAsignado.Variable)
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

            if (etapa.TipoUsuarioAsignado == TipoUsuarioAsignado.Boot)
            {
                CompletarTarea(nombreInternoProceso, nombreInternoEtapa, numeroTicket, identificacionUsuario);
            }
        }

        public void ActivarTareaHistorico(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket, string identificacionUsuario)
        {
            //Primero obtengo el proceso
            Proceso proceso = _context.Procesos.FirstOrDefault(x => x.NombreInterno == nombreInternoProceso);

            //Segundo Obtengo la etapa
            Etapa etapa = _context.Etapas.Include(e => e.Proceso).FirstOrDefault(x => x.NombreInterno == nombreInternoEtapa && x.Proceso == proceso);

            //Tercero obtengo la solicitud
            Solicitud solicitud = _context.Solicitudes.FirstOrDefault(d => d.NumeroTicket == numeroTicket);


            string usuarioAsignado = etapa.ValorUsuarioAsignado;
            if (etapa.TipoUsuarioAsignado == TipoUsuarioAsignado.Auto)
            {
                usuarioAsignado = new MotorAsignacion(_context, etapa.ValorUsuarioAsignado, numeroTicket).GetResult();
            }
            else if (etapa.TipoUsuarioAsignado == TipoUsuarioAsignado.Variable)
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

            if (etapa.TipoUsuarioAsignado == TipoUsuarioAsignado.Boot)
            {
                CompletarTareaHistorico(nombreInternoProceso, nombreInternoEtapa, numeroTicket, identificacionUsuario);
            }
        }

        public void CompletarTareaMultiHistorico(string nombreInternoProceso, string nombreInternoEtapaDestino, string numeroTicket, string identificacionUsuario)
        {

        }

        public void ActivarTareaMulti(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket, string identificacionUsuario)
        {

            Console.WriteLine(numeroTicket);
            //Primero obtengo el proceso
            Proceso proceso = _context.Procesos.FirstOrDefault(x => x.NombreInterno == nombreInternoProceso);

            //Segundo Obtengo la etapa
            Etapa etapa = _context.Etapas.FirstOrDefault(x => x.NombreInterno == nombreInternoEtapa && x.Proceso == proceso);

            //Tercero obtengo la solicitud
            Solicitud solicitud = _context.Solicitudes.FirstOrDefault(d => d.NumeroTicket == numeroTicket);
            

            string usuarioAsignado = etapa.ValorUsuarioAsignado;
            if (etapa.TipoUsuarioAsignado == TipoUsuarioAsignado.Auto)
            {
                usuarioAsignado = new MotorAsignacion(_context, etapa.ValorUsuarioAsignado, numeroTicket).GetResult();
            }
            else if (etapa.TipoUsuarioAsignado == TipoUsuarioAsignado.Variable)
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


            if (etapa.TipoUsuarioAsignado == TipoUsuarioAsignado.Boot)
            {
                CompletarTareaMulti(nombreInternoProceso, nombreInternoEtapa, numeroTicket, identificacionUsuario);
            }
        }

        public void CommitAvance()
        {
            this._context.SaveChanges();
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
            Proceso proceso = _context.Procesos.FirstOrDefault(p => p.NombreInterno == nombreInternoProceso && p.Id==1);
            Solicitud solicitud = _context.Solicitudes.Include(x => x.Tareas).ThenInclude(t => t.Etapa).FirstOrDefault(c => c.NumeroTicket.Equals(numeroTicket));
            Tarea tareaActual = solicitud.Tareas.FirstOrDefault(d => d.Etapa.NombreInterno.Equals(nombreInternoEtapa) && d.FechaTerminoFinal == null && d.Estado == EstadoTarea.Activada);

            tareaActual.EjecutadoPor = identificacionUsuario;
            tareaActual.FechaTerminoFinal = DateTime.Now;
            tareaActual.Estado = EstadoTarea.Finalizada;

            _context.Entry(tareaActual).State = EntityState.Modified;

            /*Cuando se instancia la etapa final esta cierra la solicitud */
            if (tareaActual.Etapa.TipoEtapa == TipoEtapa.Fin)
            {
                solicitud.Estado = EstadoSolicitud.Finalizada;
                solicitud.FechaTermino = DateTime.Now;
                _context.Solicitudes.Update(solicitud);
            }
            _context.SaveChanges();


            ICollection<Transito> transiciones = _context.Transiciones.Include(d => d.EtapaActaual).Include(d => d.EtapaDestino).Where(d => d.EtapaActaual.NombreInterno == nombreInternoEtapa).ToList();
            foreach (Transito transicion in transiciones)
            {
                bool estadoAvance = EjecutvaValidacion(transicion.NamespaceValidacion, transicion.ClaseValidacion, numeroTicket);
                if (estadoAvance)
                {
                    this.ActivarTarea(nombreInternoProceso, transicion.EtapaDestino.NombreInterno, numeroTicket, identificacionUsuario);

                }
            }


        }




        public void CompletarTareaHistorico(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket, string identificacionUsuario)
        {
            Proceso proceso = _context.Procesos.FirstOrDefault(p => p.NombreInterno == nombreInternoProceso);
            Solicitud solicitud = _context.Solicitudes.Include(x => x.Tareas).ThenInclude(t => t.Etapa).FirstOrDefault(c => c.NumeroTicket.Equals(numeroTicket));
            Tarea tareaActual = solicitud.Tareas.FirstOrDefault(d => d.Etapa.NombreInterno.Equals(nombreInternoEtapa) && d.FechaTerminoFinal == null && d.Estado == EstadoTarea.Activada);

            tareaActual.EjecutadoPor = identificacionUsuario;
            tareaActual.FechaTerminoFinal = DateTime.Now;
            tareaActual.Estado = EstadoTarea.Finalizada;

            _context.Entry(tareaActual).State = EntityState.Modified;

            /*Cuando se instancia la etapa final esta cierra la solicitud */
            if (tareaActual.Etapa.TipoEtapa == TipoEtapa.Fin)
            {
                solicitud.Estado = EstadoSolicitud.Finalizada;
                solicitud.FechaTermino = DateTime.Now;
                _context.Solicitudes.Update(solicitud);
            }
            _context.SaveChanges();


            ICollection<Transito> transiciones = _context.Transiciones.Include(d => d.EtapaActaual).Include(d => d.EtapaDestino).Where(d => d.EtapaActaual.NombreInterno == nombreInternoEtapa).ToList();
            foreach (Transito transicion in transiciones)
            {
                bool estadoAvance = EjecutvaValidacion(transicion.NamespaceValidacion, transicion.ClaseValidacion, numeroTicket);
                if (estadoAvance)
                {
                    this.ActivarTarea(nombreInternoProceso, transicion.EtapaDestino.NombreInterno, numeroTicket, identificacionUsuario);

                    using (var connection = new SqlConnection(_configuration.GetConnectionString("DocumentManagementConnection")))
                    {

                        string actualizar = @"

                     declare @p_id_solicitud int 
                     declare @p_unidadNegocio varchar(5)
                     declare @p_id_etapa int    
                     declare @p_existe int" +

                    " set @p_unidadNegocio=(select top 1 UnidadNegocioAsignada " +
                    " from Tareas a inner   join Solicitudes b on a.SolicitudId = b.Id " +
                    " where b.NumeroTicket='" + numeroTicket + "'" +
                    " and UnidadNegocioAsignada is not null" + " order by 1 asc)" +

                   " set @p_id_solicitud=(select id from Solicitudes where NumeroTicket='" + numeroTicket + "')" +

                          "  update a" +
                       " set a.EjecutadoPor = 'wfboot', a.Estado = 'Finalizada', a.FechaTerminoFinal = GETDATE()" +
                       " from Tareas a inner  join Solicitudes b on a.SolicitudId = b.Id" +
                       " where b.NumeroTicket = '" + numeroTicket + "'" +

                       " set @p_existe =(select COUNT(*) from Tareas where SolicitudId=@p_id_solicitud and EtapaId=18) " +
                       "  if (@p_existe=0)   " +
                       "   begin " +
                        "  insert into Tareas(SolicitudId, EtapaId, AsignadoA, ReasignadoA, EjecutadoPor, Estado, FechaInicio, FechaTerminoEstimada, FechaTerminoFinal, UnidadNegocioAsignada)  " +
                       "  values           (@p_id_solicitud,28,'wfboot',null,'wfboot','Finalizada',GETDATE(),null,GETDATE(),@p_unidadNegocio ) " +
                       "  insert into Tareas(SolicitudId, EtapaId, AsignadoA, ReasignadoA, EjecutadoPor, Estado, FechaInicio, FechaTerminoEstimada, FechaTerminoFinal, UnidadNegocioAsignada)  " +
                       "  values           (@p_id_solicitud,18,'wfboot',null,'wfboot','Finalizada',GETDATE(),null,GETDATE(),@p_unidadNegocio ) end";


                        connection.Execute(actualizar.ToString(), null, null, 240);

                    }

                }
            }


        }




        public void CompletarEtapasHistorico(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket, string identificacionUsuario)
        {


        }



        public void CompletarTareaMulti(string nombreInternoProceso, string nombreInternoEtapa, string numeroTicket, string identificacionUsuario)
        {
            try
            {
                Proceso proceso = _context.Procesos.FirstOrDefault(p => p.NombreInterno == nombreInternoProceso);
                Solicitud solicitud = _context.Solicitudes.Include(x => x.Tareas).ThenInclude(t => t.Etapa).FirstOrDefault(c => c.NumeroTicket.Equals(numeroTicket));
                Tarea tareaActual = solicitud.Tareas.FirstOrDefault(d => d.Etapa.NombreInterno.Equals(nombreInternoEtapa) && d.FechaTerminoFinal == null && d.Estado == EstadoTarea.Activada);

                

                if (tareaActual == null)
                {
                    throw new TareaNoEnEtapaException($"Tarea no en etapa, proceso/etapa/ticket:[{nombreInternoProceso}][{nombreInternoEtapa}][{numeroTicket}]");
                }


                tareaActual.EjecutadoPor = identificacionUsuario;
                tareaActual.FechaTerminoFinal = DateTime.Now;
                tareaActual.Estado = EstadoTarea.Finalizada;

                _context.Entry(tareaActual).State = EntityState.Modified;

                /*Cuando se instancia la etapa final esta cierra la solicitud */
                if (tareaActual.Etapa.TipoEtapa == TipoEtapa.Fin)
                {
                    solicitud.Estado = EstadoSolicitud.Finalizada;
                    solicitud.FechaTermino = DateTime.Now;
                    _context.Solicitudes.Update(solicitud);
                }

                ICollection<Transito> transiciones = _context.Transiciones.Include(d => d.EtapaActaual).Include(d => d.EtapaDestino).Where(d => d.EtapaActaual.NombreInterno == nombreInternoEtapa).ToList();
                foreach (Transito transicion in transiciones)
                {
                    bool estadoAvance = EjecutvaValidacion(transicion.NamespaceValidacion, transicion.ClaseValidacion, numeroTicket);
                    if (estadoAvance)
                    {
                        this.ActivarTareaMulti(nombreInternoProceso, transicion.EtapaDestino.NombreInterno, numeroTicket, identificacionUsuario);
                    }
                }

            }
            catch (Exception ex )
            {

                throw ex;
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
            Proceso proceso = _context.Procesos.Include(e => e.Etapas).FirstOrDefault(p => p.NombreInterno == nombreInternoProceso && p.Id==1);
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

            foreach (var varib in variables)
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

        public Solicitud GenerarSolicitudHistorico(string nombreInternoProceso, string identificacionUsuario, string resumen, Dictionary<string, string> variables)
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

            foreach (var varib in variables)
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

            ActivarTareaHistorico(nombreInternoProceso, etapaInicial.NombreInterno, ticket, identificacionUsuario);

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
            if (_context.Variables.Any(s => s.Clave == key && s.NumeroTicket == numeroTicket))
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
            return _context.Tareas.Include(t => t.Solicitud).ThenInclude(x => x.Proceso).Where(t => t.AsignadoA == identificacionUsuario && t.Solicitud.Proceso.NombreInterno == nombreInternoProceso && t.Estado == EstadoTarea.Activada && t.FechaTerminoFinal == null).ToList();
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



        public void ForzarAvance(string nombreInternoProceso, string nombreInternoEtapaDestino, IEnumerable<string> numeroTicket, string identificacionUsuario)
        {
            try
            {




                using (var connection = new SqlConnection(_configuration.GetConnectionString("DocumentManagementConnection")))
                {
                    foreach (var item in numeroTicket)
                    {
                        string actualizar = @"

                    declare @p_id_solicitud int 
                    declare @p_unidadNegocio varchar(5)
                    declare @p_id_etapa int
      
                   set @p_id_etapa=(select id from Etapas
					where NombreInterno='" + nombreInternoEtapaDestino + "')" +

                    " set @p_unidadNegocio=(select top 1 UnidadNegocioAsignada " +
                    " from Tareas a inner   join Solicitudes b on a.SolicitudId = b.Id " +
                    " where b.NumeroTicket='" + item.ToString() + "'" +
                    " and UnidadNegocioAsignada is not null" + " order by 1 asc)" +

                   " set @p_id_solicitud=(select id from Solicitudes where NumeroTicket='" + item.ToString() + "')" +

                          "  update a" +
                       " set a.EjecutadoPor = 'wfboot', a.Estado = 'Finalizada', a.FechaTerminoFinal = GETDATE()" +
                       " from Tareas a inner  join Solicitudes b on a.SolicitudId = b.Id" +
                       " where b.NumeroTicket = '" + item.ToString() + "'" +
                       "  insert into Tareas(SolicitudId, EtapaId, AsignadoA, ReasignadoA, EjecutadoPor, Estado, FechaInicio, FechaTerminoEstimada, FechaTerminoFinal, UnidadNegocioAsignada)  " +
                       "  values (@p_id_solicitud,@p_id_etapa,'Encargado Expedientes',null,'wfboot','Activada',GETDATE(),null,null,@p_unidadNegocio )";


                        connection.Execute(actualizar.ToString(), null, null, 240);
                    }

                }



            }
            catch (Exception)
            {

                throw new NotImplementedException();
            }

        }




    }
}
