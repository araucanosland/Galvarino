using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Galvarino.Web.Models;
using Galvarino.Web.Services.Workflow;
using Galvarino.Web.Models.Helper;
using Galvarino.Web.Data;
using Microsoft.EntityFrameworkCore;
using Galvarino.Web.Models.Application;
using Galvarino.Web.Models.Workflow;
using Microsoft.AspNetCore.Authorization;
using Galvarino.Web.Services.Notification;
using Galvarino.Web.Models.Security;
using System.Security.Claims;

namespace Galvarino.Web.Controllers.Api
{
    [Route("api/wf/v1")]
    [ApiController]
    
    public class WorkflowController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly IWorkflowService _wfService;
        private readonly INotificationKernel _mailService;
        public WorkflowController(ApplicationDbContext context, IWorkflowService wfservice, INotificationKernel mailService)
        {
            _context = context;
            _wfService = wfservice;
            _mailService = mailService;
        }

        [HttpGet("obtener-expediente/{folioCredito}/{etapaSolicitud?}")]
        public IActionResult ObtenerXpediente([FromRoute] string folioCredito, [FromRoute] string etapaSolicitud = "")
        {
            /* TODO Validar que sea de la oficina y que este en la etapa correcta de solicitud*/
            var expediente = _context.ExpedientesCreditos.Include(x => x.Credito).Include(x => x.Documentos).FirstOrDefault(x => x.Credito.FolioCredito == folioCredito);
            var oficinaUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
            var ofipago = _wfService.ObtenerVariable("OFICINA_PAGO", expediente.Credito.NumeroTicket);
            var laOficinaPago = _context.Oficinas.Include(k => k.OficinaProceso).FirstOrDefault(ofi => ofi.Codificacion == ofipago);



            if (expediente.Documentos.Count() > 0 && ((oficinaUsuario == "A000") || (ofipago == oficinaUsuario || laOficinaPago.OficinaProceso.Codificacion == oficinaUsuario)))
            {
                if (!string.IsNullOrEmpty(etapaSolicitud))
                {
                    var etapaActual = _wfService.OntenerTareaActual("PROCESS", expediente.Credito.NumeroTicket);
                    if(etapaActual.Etapa.NombreInterno == etapaSolicitud)
                    {
                        return Ok(expediente);
                    }
                    else
                    {
                        return NotFound("Este Expediente esta en la siguiente tarea: <strong>" + etapaActual.Etapa.Nombre + "</strong>");
                    }
                }
                else
                {
                    return Ok(expediente);
                }
            }
            else
            {
                return NotFound("Carga del Expediente en: " + laOficinaPago.Nombre);
            }
        }

        [HttpGet("mis-solicitudes/{etapaIn?}")]
        public async Task<IActionResult> ListarMisSolicitudes([FromRoute] string etapaIn = "", [FromQuery] int offset = 0, [FromQuery] int limit=20)
        {

            //var rolUsuario =  //FirstOrDefault(x => x.Type == ClaimTypes.Role).Value;    
            var oficinaUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value; 
            var ObjetoOficinaUsuario = _context.Oficinas.Include(of=> of.OficinaProceso).FirstOrDefault(ofc => ofc.Codificacion == oficinaUsuario);
            var OficinasUsuario = _context.Oficinas.Where(ofc => ofc.OficinaProceso.Id == ObjetoOficinaUsuario.Id && ofc.EsMovil == true);
            var ofinales = new List<Oficina>();
            ofinales.Add(ObjetoOficinaUsuario);
            ofinales.AddRange(OficinasUsuario.ToList());


            /*Optimizando */
            var laSalida = from tarea in _context.Tareas
                            join etapa in _context.Etapas on tarea.Etapa equals etapa
                            join solicitud in _context.Solicitudes on tarea.Solicitud equals solicitud
                            join credito in _context.Creditos on solicitud.NumeroTicket equals credito.NumeroTicket
                            join expediente in _context.ExpedientesCreditos.Include(exp => exp.Documentos) on credito equals expediente.Credito
                            join packnota in _context.PacksNotarias on expediente.PackNotaria equals packnota into pks
                            join valijaval in _context.ValijasValoradas on expediente.ValijaValorada equals valijaval into vljs
                            from packnota in pks.DefaultIfEmpty()
                            from valijaval in vljs.DefaultIfEmpty()


                where (
                                ((tarea.AsignadoA == User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", "") || etapa.TipoUsuarioAsignado == TipoUsuarioAsignado.Rol && User.IsInRole(tarea.AsignadoA))
                                    && ((tarea.UnidadNegocioAsignada != null && ofinales.Select(ofs => ofs.Codificacion).Contains(tarea.UnidadNegocioAsignada)) || tarea.UnidadNegocioAsignada == null))

                                && tarea.Estado == EstadoTarea.Activada
                                && ((!string.IsNullOrEmpty(etapaIn) && etapa.NombreInterno == etapaIn) || (string.IsNullOrEmpty(etapaIn)))
                            )
                select new
                {
                    
                    credito.FolioCredito,
                    credito.RutCliente,
                    credito.TipoCredito,
                    expediente.Documentos,
                    credito.FechaDesembolso,
                    seguimientoNotaria = packnota !=null ? packnota.CodigoSeguimiento : "",
                    fechaEnvioNotaria = packnota != null ? packnota.FechaEnvio : DateTime.MinValue,
                    seguimientoValija = valijaval != null ? valijaval.CodigoSeguimiento : "",
                    fechaEnvioValija = valijaval != null ? valijaval.FechaEnvio : DateTime.MinValue,
                    reparo = _context.Variables.FirstOrDefault(vari => vari.NumeroTicket == tarea.Solicitud.NumeroTicket && vari.Clave == "CODIGO_MOTIVO_DEVOLUCION_A_SUCURSAL").Valor,
                    reparoNotaria = _context.Variables.FirstOrDefault(vari => vari.NumeroTicket == tarea.Solicitud.NumeroTicket && vari.Clave == "REPARO_REVISION_DOCUMENTO_LEGALIZADO").Valor,
                    documentosFaltantes = _context.Variables.FirstOrDefault(vari => vari.NumeroTicket == tarea.Solicitud.NumeroTicket && vari.Clave == "COLECCION_DOCUMENTOS_FALTANTES").Valor
                };

            
            var salida = await laSalida.ToListAsync();
            var lida = new {
                total = salida.Count(),
                rows = salida.Skip(offset).Take(limit).ToList()
            };
            
            return Ok(lida);
        }

        [HttpGet("mis-solicitudes-mesa-control/{etapaIn?}")]
        public async Task<IActionResult> ListarMisSolicitudesMSC([FromRoute] string etapaIn = "", [FromQuery] int offset = 0, [FromQuery] int limit = 20)
        {

            //var rolUsuario =  //FirstOrDefault(x => x.Type == ClaimTypes.Role).Value;    
            var oficinaUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
            var ObjetoOficinaUsuario = _context.Oficinas.Include(of => of.OficinaProceso).FirstOrDefault(ofc => ofc.Codificacion == oficinaUsuario);
            var OficinasUsuario = _context.Oficinas.Where(ofc => ofc.OficinaProceso.Id == ObjetoOficinaUsuario.Id && ofc.EsMovil == true);
            var ofinales = new List<Oficina>();
            ofinales.Add(ObjetoOficinaUsuario);
            ofinales.AddRange(OficinasUsuario.ToList());


            /*Optimizando */
            var laSalida = from tarea in _context.Tareas
                           join etapa in _context.Etapas on tarea.Etapa equals etapa
                           join solicitud in _context.Solicitudes on tarea.Solicitud equals solicitud
                           join credito in _context.Creditos on solicitud.NumeroTicket equals credito.NumeroTicket
                           join expediente in _context.ExpedientesCreditos.Include(exp => exp.Documentos) on credito equals expediente.Credito
                           

                           where (
                                           ((tarea.AsignadoA == User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", "") || etapa.TipoUsuarioAsignado == TipoUsuarioAsignado.Rol && User.IsInRole(tarea.AsignadoA))
                                               && ((tarea.UnidadNegocioAsignada != null && ofinales.Select(ofs => ofs.Codificacion).Contains(tarea.UnidadNegocioAsignada)) || tarea.UnidadNegocioAsignada == null))

                                           && tarea.Estado == EstadoTarea.Activada
                                           && ((!string.IsNullOrEmpty(etapaIn) && etapa.NombreInterno == etapaIn) || (string.IsNullOrEmpty(etapaIn)))
                                       )
                           select new
                           {
                               credito.FolioCredito,
                               credito.RutCliente,
                               credito.TipoCredito,
                               expediente.Documentos,
                               credito.FechaDesembolso
                           };


            var salida = await laSalida.ToListAsync();
            var lida = new
            {
                total = salida.Count(),
                rows = salida.Skip(offset).Take(limit).ToList()
            };

            return Ok(lida);
        }

        [HttpGet("mi-resumen")]
        public IActionResult MiResumen()
        {
            var salida = new List<dynamic>();
            //User.IsInRole("")

            var oficinaUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
            var ObjetoOficinaUsuario = _context.Oficinas.Include(of => of.OficinaProceso).FirstOrDefault(ofc => ofc.Codificacion == oficinaUsuario);
            var OficinasUsuario = _context.Oficinas.Where(ofc => ofc.OficinaProceso.Id == ObjetoOficinaUsuario.Id && ofc.EsMovil == true);
            var ofinales = new List<Oficina>();
            ofinales.Add(ObjetoOficinaUsuario);
            ofinales.AddRange(OficinasUsuario.ToList());



            //var oficinaUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
            //var oficinas = _context.Oficinas.Include(d => d.OficinaProceso).Where(ofi => ofi.Id == ofi.OficinaProceso.Id && ofi.Codificacion == oficinaUsuario);
            var tareas = _context.Tareas.Include(d => d.Etapa).Include(f => f.Solicitud).Where(x => x.Estado == EstadoTarea.Activada);
            var expedientes = _context.ExpedientesCreditos
                                .Include(f => f.Documentos)
                                .Include(f => f.Credito);

            var offices = from off in ofinales
                          join tar in tareas on off.Codificacion equals tar.UnidadNegocioAsignada
                          join exp in expedientes on tar.Solicitud.NumeroTicket equals exp.Credito.NumeroTicket
                          group new { tar, exp } by new {Tarea = tar.Etapa.Nombre } into grp
                          select new
                          {
                              Tarea = grp.Key.Tarea,
                              Total = grp.Count()
                          };

            //var ret = await offices.ToArrayAsync();
            return Ok(offices);            
        }

        [HttpGet("mis-solicitudes/{etapaIn}/{notaria}")]
        public async Task<IActionResult> ListarMisSolicitudesNotaria([FromRoute] string etapaIn, [FromRoute] int notaria, [FromQuery] int offset = 0, [FromQuery] int limit = 20)
        {

            //var rolUsuario =  //FirstOrDefault(x => x.Type == ClaimTypes.Role).Value;    
            var oficinaUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
            var ObjetoOficinaUsuario = _context.Oficinas.Include(of => of.OficinaProceso).FirstOrDefault(ofc => ofc.Codificacion == oficinaUsuario);
            var OficinasUsuario = _context.Oficinas.Where(ofc => ofc.OficinaProceso.Id == ObjetoOficinaUsuario.Id && ofc.EsMovil == true);
            var ofinales = new List<Oficina>();
            ofinales.Add(ObjetoOficinaUsuario);
            ofinales.AddRange(OficinasUsuario.ToList());


            /*Optimizando */
            var laSalida = from tarea in _context.Tareas
                           join etapa in _context.Etapas on tarea.Etapa equals etapa
                           join solicitud in _context.Solicitudes on tarea.Solicitud equals solicitud
                           join credito in _context.Creditos on solicitud.NumeroTicket equals credito.NumeroTicket
                           join expediente in _context.ExpedientesCreditos.Include(exp => exp.Documentos) on credito equals expediente.Credito
                           join packnota in _context.PacksNotarias.Include(p => p.NotariaEnvio) on expediente.PackNotaria equals packnota
                           

                           where (
                                           ((tarea.AsignadoA == User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", "") || etapa.TipoUsuarioAsignado == TipoUsuarioAsignado.Rol && User.IsInRole(tarea.AsignadoA))
                                               && ((tarea.UnidadNegocioAsignada != null && ofinales.Select(ofs => ofs.Codificacion).Contains(tarea.UnidadNegocioAsignada)) || tarea.UnidadNegocioAsignada == null))
                                       )
                                && tarea.Estado == EstadoTarea.Activada
                                && etapa.NombreInterno == etapaIn
                                && packnota.NotariaEnvio.Id == notaria
                               
                           select new
                           {

                               credito.FolioCredito,
                               credito.RutCliente,
                               credito.TipoCredito,
                               expediente.Documentos,
                               credito.FechaDesembolso,
                               seguimientoNotaria = packnota != null ? packnota.CodigoSeguimiento : "",
                               fechaEnvioNotaria = packnota != null ? packnota.FechaEnvio : DateTime.MinValue,
                               seguimientoValija = "",
                               fechaEnvioValija = DateTime.MinValue,
                               reparo = _context.Variables.FirstOrDefault(vari => vari.NumeroTicket == tarea.Solicitud.NumeroTicket && vari.Clave == "CODIGO_MOTIVO_DEVOLUCION_A_SUCURSAL").Valor,
                               reparoNotaria = _context.Variables.FirstOrDefault(vari => vari.NumeroTicket == tarea.Solicitud.NumeroTicket && vari.Clave == "REPARO_REVISION_DOCUMENTO_LEGALIZADO").Valor,
                               documentosFaltantes = _context.Variables.FirstOrDefault(vari => vari.NumeroTicket == tarea.Solicitud.NumeroTicket && vari.Clave == "COLECCION_DOCUMENTOS_FALTANTES").Valor
                           };


            var salida = await laSalida.ToListAsync();
            var lida = new
            {
                total = salida.Count(),
                rows = salida.Skip(offset).Take(limit).ToList()
            };

            return Ok(lida);
        }

        [HttpGet("documentos-generados/{tipoDocumento}")]
        public async Task<IActionResult> ListarMisDocumentosGenerados(string tipoDocumento)
        {

            var oficinaUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
            var ObjetoOficinaUsuario = await _context.Oficinas.Include(of => of.OficinaProceso).FirstOrDefaultAsync(ofc => ofc.OficinaProceso.Codificacion == oficinaUsuario);
            var OficinasUsuario = _context.Oficinas.Where(ofc => ofc.OficinaProceso.Id == ObjetoOficinaUsuario.Id);

            switch (tipoDocumento)
            {
                case "nomina-notaria":
                    var packNotaria = await _context.PacksNotarias.Include(p => p.Expedientes).Include(p => p.Oficina).Where(p => p.Oficina.Codificacion == oficinaUsuario).ToListAsync();
                    return Ok(ObjetoOficinaUsuario.PacksNotaria);

                case "valija-oficina":
                    var valijaOficina = await _context.ValijasOficinas.Include(d => d.OficinaEnvio).Include(d => d.Expedientes).Where(x => x.OficinaEnvio.Codificacion == oficinaUsuario).ToListAsync();
                    return Ok(valijaOficina);

                case "valija-valorada":
                    var valijaValodara = await _context.ValijasValoradas.Include(d => d.Oficina).Include(d => d.Expedientes).Where(x => x.Oficina.Codificacion == oficinaUsuario).ToListAsync();
                    return Ok(valijaValodara);

                case "caja-valorada":
                    var cajaValodara = await _context.CajasValoradas.Include(d => d.Expedientes).Where(x => x.MarcaAvance == "DESPACUST").ToListAsync();
                    return Ok(cajaValodara);

                default:
                    throw new Exception("Debes Ingresar con una opcion de mostrado");
            }
        }

        [HttpGet("solicitudes-reparo-devolicion/{oficina?}")]
        public async Task<IActionResult> ListarOficinasReparo(string oficina="")
        {

            var mistareas = _context.Tareas.Include(d => d.Etapa).Include(f => f.Solicitud).Where(x => (x.AsignadoA == User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", "") || x.Etapa.TipoUsuarioAsignado == TipoUsuarioAsignado.Rol && User.IsInRole(x.AsignadoA)) && x.Estado == EstadoTarea.Activada && x.Etapa.NombreInterno == "DESPACHO_OF_PARTES_DEVOLUCION");

            var salida = new List<dynamic>();
            await mistareas.ForEachAsync(tarea =>
            {

                var folioCredito = _wfService.ObtenerVariable("FOLIO_CREDITO", tarea.Solicitud.NumeroTicket);
                var codigoOficinaDevolucion = _wfService.ObtenerVariable("OFICINA_PROCESA_NOTARIA", tarea.Solicitud.NumeroTicket);

                var oficinaDevolucion = _context.Oficinas.FirstOrDefault(o => o.Codificacion == codigoOficinaDevolucion);
                var credito = _context.Creditos.FirstOrDefault(cre => cre.FolioCredito == folioCredito);
                var expediente = _context.ExpedientesCreditos
                        .Include(f => f.Documentos)
                        .Include(f => f.Credito)
                        .Include(s => s.ValijaValorada)
                        .FirstOrDefault(ex => ex.Credito.FolioCredito == folioCredito);
                

                salida.Add(new
                {
                    tarea = tarea,
                    expediente = expediente,
                    oficinaDevolucion = oficinaDevolucion
                });
            });

            if(!string.IsNullOrEmpty(oficina)){
                salida = salida.Where(s => s.oficinaDevolucion.Codificacion == oficina).ToList();
            }

            return Ok(salida);
        }



        [HttpPost("despacho-oficina-matriz")]
        public async Task<IActionResult> DespachoOfMatriz([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            var tran  = _context.Database.BeginTransaction();
            try{
                List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
                List<string> ticketsAvanzar = new List<string>();

                var codificacionOficinaLogedIn = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
                var oficinaEnvio = _context.Oficinas.Include(fd => fd.OficinaProceso).FirstOrDefault(d => d.Codificacion == codificacionOficinaLogedIn);


                DateTime now = DateTime.Now;
                var codSeg = now.Ticks.ToString() + "O" + oficinaEnvio.Codificacion;

                var valijaMatrix = new ValijaOficina
                {
                    CodigoSeguimiento = codSeg,
                    FechaEnvio = DateTime.Now,
                    OficinaEnvio = oficinaEnvio,
                    OficinaDestino = oficinaEnvio.OficinaProceso,
                    MarcaAvance = "INI"
                };


                foreach (var item in entrada)
                {
                    var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                    elExpediente.ValijaOficina = valijaMatrix;
                    expedientesModificados.Add(elExpediente);
                    ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
                }

                _context.ValijasOficinas.Add(valijaMatrix);
                _context.ExpedientesCreditos.UpdateRange(expedientesModificados);
                await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DESPACHO_OFICINA_NOTARIA, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));

                await _context.SaveChangesAsync();
                tran.Commit();
                return Ok(valijaMatrix);
            }
            catch(Exception)
            {
                tran.Rollback();
                return BadRequest();
            }
            
        }

        [HttpPost("recepcion-expedientes-matriz/{codigoSeguimiento}")]
        public async Task<IActionResult> RecepcionSetLegal([FromBody] IEnumerable<ExpedienteGenerico> entrada, [FromRoute] string codigoSeguimiento)
        {
            var tran = _context.Database.BeginTransaction();
            try
            {
                List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
                List<string> ticketsAvanzar = new List<string>();
                
                foreach (var item in entrada)
                {
                    
                    var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                    ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
                }
                
                await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_RECEPCION_SET_LEGAL, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));
                var laValija = _context.ValijasOficinas.FirstOrDefault(v => v.CodigoSeguimiento == codigoSeguimiento);
                laValija.MarcaAvance = "OK";
                _context.ValijasOficinas.Update(laValija);

                await _context.SaveChangesAsync();
                tran.Commit();
                return Ok();
            }
            catch (Exception)
            {
                tran.Rollback();
                return BadRequest();
            }
        }

        [HttpPost("envio-a-notaria")]
        public async Task<IActionResult> EnvioNotaria([FromBody] ColeccionExpedientesGenerica entrada)
        {
            var tran = _context.Database.BeginTransaction();
            try
            {
                List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
                List<string> ticketsAvanzar = new List<string>();

                var codificacionOficinaLogedIn = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
                var oficinaEnvio = _context.Oficinas.Include(ofi => ofi.Comuna).FirstOrDefault(d => d.Codificacion == codificacionOficinaLogedIn);
                var notariaEnvio = _context.Notarias.FirstOrDefault(d => d.Id == entrada.CodNotaria);
                            
                DateTime now = DateTime.Now;
                var codSeg = now.Ticks.ToString() + "N" + notariaEnvio.Id.ToString().PadLeft(2, '0').ToString();
                
                
                var packNotaria = new PackNotaria
                {
                    FechaEnvio = DateTime.Now,
                    NotariaEnvio = notariaEnvio,
                    Oficina = oficinaEnvio,
                    CodigoSeguimiento = codSeg
                };

                _context.PacksNotarias.Add(packNotaria);

                foreach (var item in entrada.ExpedientesGenericos)
                {
                    var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                    elExpediente.PackNotaria = packNotaria;
                    expedientesModificados.Add(elExpediente);
                    ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
                }

                _context.ExpedientesCreditos.UpdateRange(expedientesModificados);
                await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_ENVIO_NOTARIA, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));

                await _context.SaveChangesAsync();

                tran.Commit();
                return Ok(packNotaria);
            }
            catch(Exception)
            {
                tran.Rollback();
                return BadRequest();
            }
        }

        [HttpPost("recepciona-notaria")]
        public async Task<IActionResult> RecepcionaNotaria([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            var tran = _context.Database.BeginTransaction();
            try
            {
                List<string> ticketsAvanzar = new List<string>();

                foreach (var item in entrada)
                {
                    var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                    ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
                }

                await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_RECEPCION_NOTARIA, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));

                await _context.SaveChangesAsync();
                tran.Commit();
                return Ok();
            }
            catch(Exception)
            {
                tran.Rollback();
                return BadRequest();
            }
        }

        [HttpPost("revision-documentos")]
        public async Task<IActionResult> RevisionDocumentos([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            var tran = _context.Database.BeginTransaction();
            try
            {
                List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
                List<string> ticketsAvanzar = new List<string>();

                foreach (var item in entrada)
                {
                    var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                    _wfService.AsignarVariable("REPARO_REVISION_DOCUMENTO_LEGALIZADO", item.Reparo.ToString(), elExpediente.Credito.NumeroTicket);
                    ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
                }

                await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_REVISION_DOCUMENTOS, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));
                await _context.SaveChangesAsync();
                tran.Commit();
                return Ok(entrada);
            }
            catch(Exception)
            {
                tran.Rollback();
                return BadRequest();
            }
        }

        [HttpPost("despacho-reparo-notaria")]
        public async Task<IActionResult> DespachoReparoNotaria([FromBody] ColeccionEnvioNotariaFormHelper entrada)
        {
            var tran = _context.Database.BeginTransaction();
            try
            {
                List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
                List<string> ticketsAvanzar = new List<string>();

                var codificacionOficinaLogedIn = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
                var oficinaEnvio = _context.Oficinas.Include(of => of.Comuna).FirstOrDefault(d => d.Codificacion == codificacionOficinaLogedIn);
                var notariaEnvio = entrada.Notaria > 0 ? _context.Notarias.FirstOrDefault(not=> not.Id == entrada.Notaria) : _context.Notarias.Include(nt => nt.Comuna).FirstOrDefault(not => not.Comuna.Id == oficinaEnvio.Comuna.Id);
                

                DateTime now = DateTime.Now;
                var codSeg = now.Ticks.ToString() + "R" + notariaEnvio.Id.ToString().PadLeft(2, '0');

                var packNotaria = new PackNotaria
                {
                    FechaEnvio = DateTime.Now,
                    NotariaEnvio = notariaEnvio,
                    Oficina = oficinaEnvio,
                    CodigoSeguimiento = codSeg
                };

                _context.PacksNotarias.Add(packNotaria);

                foreach (var item in entrada.Expedientes)
                {
                    var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                    elExpediente.PackNotaria = packNotaria;
                    expedientesModificados.Add(elExpediente);
                    ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
                }

                _context.ExpedientesCreditos.UpdateRange(expedientesModificados);
                await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DESPACHO_REPARO_NOTARIA, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));
                await _context.SaveChangesAsync();
                tran.Commit();
                return Ok(packNotaria);
            }
            catch(Exception)
            {
                tran.Rollback();
                return BadRequest();
            }
        }

        [HttpPost("despacho-sucursal-oficiana-partes")]
        public async Task<IActionResult> DespachoSucOfPartes([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            var tran = _context.Database.BeginTransaction();
            try
            {
                List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
                List<string> ticketsAvanzar = new List<string>();

                var codificacionOficinaLogedIn = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
                var oficinaEnvio = _context.Oficinas.FirstOrDefault(d => d.Codificacion == codificacionOficinaLogedIn);
                
                DateTime now = DateTime.Now;
                var codSeg = now.Ticks.ToString() + "X" + oficinaEnvio.Codificacion; 

                var valijaEnvio = new ValijaValorada
                {
                    FechaEnvio = DateTime.Now,
                    Oficina = oficinaEnvio,
                    CodigoSeguimiento=codSeg,
                    MarcaAvance = "OF_PARTES"
                };

                _context.ValijasValoradas.Add(valijaEnvio);
                
                foreach (var item in entrada)
                {
                    var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                    elExpediente.ValijaValorada = valijaEnvio;
                    expedientesModificados.Add(elExpediente);
                    _wfService.AsignarVariable("USUARIO_DESPACHA_A_OF_PARTES", User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""), elExpediente.Credito.NumeroTicket);
                    ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
                }

                _context.ExpedientesCreditos.UpdateRange(expedientesModificados);
                await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DESPACHO_OFICINA_PARTES, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));
                await _context.SaveChangesAsync();
                tran.Commit();
                return Ok(valijaEnvio);
            }
            catch(Exception)
            {
                tran.Rollback();
                return BadRequest();
            }
        }

        [HttpGet("recepcion-oficina-partes/{codigoSeguimiento}")]
        public async Task<IActionResult> RecepcionOfPartes([FromRoute] string codigoSeguimiento)
        {
            var tran = _context.Database.BeginTransaction();
            try
            {
                List<string> ticketsAvanzar = new List<string>();

                var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).Include(d => d.ValijaValorada).Where(x => x.ValijaValorada.CodigoSeguimiento == codigoSeguimiento);

                foreach(var item in elExpediente)
                {
                    ticketsAvanzar.Add(item.Credito.NumeroTicket);
                }

                await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_RECEPCION_VALIJA_OFICINA_PARTES, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));

                var laValija = _context.ValijasValoradas.FirstOrDefault(v => v.CodigoSeguimiento == codigoSeguimiento);
                laValija.MarcaAvance = "MS_CONTROL";
                _context.ValijasValoradas.Update(laValija);
                await _context.SaveChangesAsync();
                tran.Commit();
                return Ok();
            }
            catch (Exception)
            {
                tran.Rollback();
                return BadRequest();
            }
        }

        [HttpGet("recepcion-valija/{codigoSeguimiento}")]
        public async Task<IActionResult> RecepcionValija([FromRoute] string codigoSeguimiento)
        {
            var tran = _context.Database.BeginTransaction();
            try
            {
                List<string> ticketsAvanzar = new List<string>();
                var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).Include(d => d.ValijaValorada).Where(x => x.ValijaValorada.CodigoSeguimiento == codigoSeguimiento);

                foreach (var item in elExpediente)
                {
                    ticketsAvanzar.Add(item.Credito.NumeroTicket);
                }

                await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_RECEPCION_VALIJA_MESA_CONTROL, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));

                var laValija = _context.ValijasValoradas.FirstOrDefault(v => v.CodigoSeguimiento == codigoSeguimiento);
                laValija.MarcaAvance = "MSC_APERTURA";
                _context.ValijasValoradas.Update(laValija);
                await _context.SaveChangesAsync();

                tran.Commit();
                return Ok();
            }
            catch (Exception)
            {
                tran.Rollback();
                return BadRequest();
            }
        }


        [HttpPost("apertura-valija/{codigoSeguimiento}")]
        public async Task<IActionResult> AperturaValija([FromBody] IEnumerable<ExpedienteGenerico> entrada, [FromRoute] string codigoSeguimiento)
        {
            var tran = _context.Database.BeginTransaction();
            try
            {
                List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
                List<string> ticketsAvanzar = new List<string>();


                foreach (var item in entrada)
                {
                    var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).Include(d => d.Documentos).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                    _wfService.AsignarVariable("EXPEDIENTE_FALTANTE", item.Faltante ? 1.ToString() : 0.ToString(), elExpediente.Credito.NumeroTicket);
                    
                    if(item.Faltante)
                    {
                        string jsonEnv = ""; int controlador = 0; string enviocorreo="";
                        
                        foreach(var dc in elExpediente.Documentos)
                        {
                            if(!item.DocumentosPistoleados.Any(d => d == dc.Codificacion))
                            {
                                jsonEnv = jsonEnv + (controlador > 0 ? "," : "") + "\"" + dc.Codificacion + "\"";
                                enviocorreo+= (controlador > 0 ? ", " : "") + dc.TipoDocumento.ToString(); 
                                controlador++;
                            }
                        }

                        jsonEnv = "[" + jsonEnv + "]";
                        _wfService.AsignarVariable("COLECCION_DOCUMENTOS_FALTANTES", jsonEnv, elExpediente.Credito.NumeroTicket);
                        //enviocorreo = jsonEnv.Replace("[","").Replace("]","").Replace("'","");
                        string mensaje = @"
                        <table>
                            <tr>
                                <td><strong>Folio</strong></td>
                                <td><strong>Reparo</strong></td>
                            </tr>
                            <tr>
                                <td>" + item.FolioCredito + @"</td>
                                <td>Codigo Documento(s) Faltante(s): " + enviocorreo + @"</td>
                            </tr>
                        </table>";
                        var enviarA = _wfService.QuienCerroEtapa(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DESPACHO_OFICINA_PARTES, elExpediente.Credito.NumeroTicket);
                        await _mailService.SendEmail(enviarA.NormalizedEmail, "Notificación de Expediente con Faltantes: " + item.FolioCredito, mensaje);
                    }
                    
                    ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
                }

                await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_APERTURA_VALIJA, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));


                var laValija = _context.ValijasValoradas.FirstOrDefault(v => v.CodigoSeguimiento == codigoSeguimiento);
                laValija.MarcaAvance = "FN";
                _context.ValijasValoradas.Update(laValija);
                await _context.SaveChangesAsync();

                tran.Commit();
                return Ok();
            }
            catch (Exception)
            {
                tran.Rollback();
                return BadRequest();
            }
        }

        [Route("analisis-mesa-control")]
        public async Task<IActionResult> AnalisisMesaControl([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            var tran = _context.Database.BeginTransaction();
            try
            {
                List<string> ticketsAvanzar = new List<string>();
                List<EnvioNotariaFormHelper> expedientesReparo = new List<EnvioNotariaFormHelper>();
                var opt = new string[] { "Sin Reparos", "Sin Firma de Notario", "Sin Timbre de Notario", "Sin Firma ni Timbre", "Ilegible" };
                foreach (var item in entrada)
                {
                    var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                    _wfService.AsignarVariable("DEVOLUCION_A_SUCURSAL", item.Reparo > 0 ? 1.ToString() : 0.ToString(), elExpediente.Credito.NumeroTicket);
                    if(item.Reparo > 0){
                        _wfService.AsignarVariable("CODIGO_MOTIVO_DEVOLUCION_A_SUCURSAL", item.Reparo.ToString(), elExpediente.Credito.NumeroTicket);
                        _wfService.AsignarVariable("TEXTO_MOTIVO_DEVOLUCION_A_SUCURSAL", opt[item.Reparo], elExpediente.Credito.NumeroTicket);
                        string mensaje = @"
                        <table>
                            <tr>
                                <td><strong>Folio</strong></td>
                                <td><strong>Reparo</strong></td>
                            </tr>
                            <tr>
                                <td>" + item.FolioCredito + @"</td>
                                <td>El Expediente contiene un documento " + opt[item.Reparo] + @"</td>
                            </tr>
                        </table>";
                        var enviarA = _wfService.QuienCerroEtapa(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DESPACHO_OFICINA_PARTES, elExpediente.Credito.NumeroTicket);
                        await _mailService.SendEmail(enviarA.NormalizedEmail, "Notificación de Expediente con Reparos: " + item.FolioCredito, mensaje);
                    }
                    ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
                }

                await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_ANALISIS_MESA_CONTROL, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));
                await _context.SaveChangesAsync();
                tran.Commit();
                return Ok();
            }
            catch (Exception)
            {
                tran.Rollback();
                return BadRequest();
            }
        }
        //TODO Avanzar con la terminacion de la insercion de transaccion
        [Route("solucion-expediente-faltante")]
        public async Task<IActionResult> SolucionExpedienteFaltante([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            var tran = _context.Database.BeginTransaction();
            try
            {
                List<string> ticketsAvanzar = new List<string>();

                foreach (var item in entrada)
                {
                    var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                    _wfService.AsignarVariable("DOCUMENTACION_FALTANTE_EN_TRANSITO", 1.ToString(), elExpediente.Credito.NumeroTicket);
                    ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
                }
                await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DOCUMENTACION_FALTANTE, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));
                await _context.SaveChangesAsync();
                tran.Commit();
                return Ok();
            }
            catch (Exception)
            {
                tran.Rollback();
                return BadRequest();
            }
        }

        [Route("despacho-reparo-oficina-partes")]
        public async Task<IActionResult> DespachoOfPtReparoSucursal([FromBody] ColeccionExpedientesGenerica entrada)
        {
            var tran = _context.Database.BeginTransaction();
            try
            {
                List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
                List<string> ticketsAvanzar = new List<string>();

                var oficinaEnvio = _context.Oficinas.FirstOrDefault(o => o.Codificacion == entrada.CodOficina);


                DateTime now = DateTime.Now;
                var codSeg = now.Ticks.ToString() + "Z" + oficinaEnvio.Codificacion;

                var valijaEnvio = new ValijaValorada
                {
                    FechaEnvio = DateTime.Now,
                    Oficina = oficinaEnvio,
                    CodigoSeguimiento = codSeg,
                    MarcaAvance = "DEVOLVER"
                };

                _context.ValijasValoradas.Add(valijaEnvio);

                foreach (var item in entrada.ExpedientesGenericos)
                {
                    var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                    elExpediente.ValijaValorada = valijaEnvio;
                    expedientesModificados.Add(elExpediente);
                    ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
                }

                _context.ExpedientesCreditos.UpdateRange(expedientesModificados);
                
                await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DESPACHO_OF_PARTES_DEVOLUCION, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));
                await _context.SaveChangesAsync();
                tran.Commit();
                return Ok(valijaEnvio);
            }
            catch (Exception)
            {
                tran.Rollback();
                return BadRequest();
            }
        }

        [HttpGet("despacho-oficina-correccion/{codigoSeguimiento}")]
        public async Task<IActionResult> DespachoOfCorreccion([FromRoute] string codigoSeguimiento)
        {
            var tran = _context.Database.BeginTransaction();
            try
            {
                List<string> ticketsAvanzar = new List<string>();
                var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).Include(d => d.ValijaValorada).Where(x => x.ValijaValorada.CodigoSeguimiento == codigoSeguimiento);

                foreach (var item in elExpediente)
                {
                    ticketsAvanzar.Add(item.Credito.NumeroTicket);
                }

                await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DESPACHO_OFICINA_CORRECCION, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));

                var laValija = _context.ValijasValoradas.FirstOrDefault(v => v.CodigoSeguimiento == codigoSeguimiento);
                laValija.MarcaAvance = "DEVOLUCION_OP";
                _context.ValijasValoradas.Update(laValija);
                await _context.SaveChangesAsync();

                tran.Commit();
                return Ok();
            }
            catch (Exception)
            {
                tran.Rollback();
                return BadRequest();
            }
        }

        [HttpPost("solucion-reparo-sucursal")]
        public async Task<IActionResult> SolucionReparosSucursal([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            var tran = _context.Database.BeginTransaction();
            try
            {
                List<string> ticketsAvanzar = new List<string>();
                foreach (var item in entrada)
                {
                    var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                    ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
                }

                await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_SOLUCION_REPAROS_SUCURSAL, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));
                await _context.SaveChangesAsync();
                tran.Commit();
                return Ok();
            }
            catch (Exception)
            {
                tran.Rollback();
                return BadRequest();
            }
        }

        [Route("despacho-a-custodia/{codigoCaja}")]
        public async Task<IActionResult> DespachoCustodia([FromRoute] string codigoCaja)
        {
            var tran = _context.Database.BeginTransaction();
            try
            {
                //List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
                //List<string> ticketsAvanzar = new List<string>();
                //DateTime now = DateTime.Now;
                //var codSeg = now.Ticks.ToString() + "CV" ;

                var cajaval = _context.CajasValoradas.FirstOrDefault(d => d.CodigoSeguimiento == codigoCaja && d.Usuario == User.Identity.Name);
                /*var folios = _context.PasosValijasValoradas.Where(c => c.CodigoCajaValorada == codigoCaja && c.Usuario == User.Identity.Name).GroupBy(d => d.FolioCredito).Select(d => d.Key).ToList();

                foreach (var item in folios)
                {
                    var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item);
                    elExpediente.CajaValorada = cajaval;
                    expedientesModificados.Add(elExpediente);
                    ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
                }*/
                //Sacar la caja del estado buffer
                cajaval.MarcaAvance = "READYTOPROCESS";
                _context.CajasValoradas.Update(cajaval);
                //_context.ExpedientesCreditos.UpdateRange(expedientesModificados);
                await _context.SaveChangesAsync();
                //await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DESPACHO_A_CUSTODIA, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));

                //_context.PasosValijasValoradas.RemoveRange(_context.PasosValijasValoradas.Where(c => c.CodigoCajaValorada == codigoCaja && c.Usuario == User.Identity.Name));
                //await _context.SaveChangesAsync();
                tran.Commit();
                return Ok();
            }
            catch (Exception ex)
            {
                tran.Rollback();
                return BadRequest(ex.Message);
            }
        }

        [Route("despacho-a-custodia/generar-caja-valorada")]
        public async Task<IActionResult> GenerarCajaValorada()
        {
            try
            {
                var existe = _context.CajasValoradas.FirstOrDefault(ca => ca.Usuario == User.Identity.Name && ca.MarcaAvance == "BUFFER");
                if(existe == null)
                {
                    List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
                    List<string> ticketsAvanzar = new List<string>();
                    DateTime now = DateTime.Now;

                    var codSeg = now.Ticks.ToString() + "CV";

                    CajaValorada cajaval = new CajaValorada
                    {
                        FechaEnvio = DateTime.Now,
                        CodigoSeguimiento = codSeg,
                        MarcaAvance = "BUFFER",
                        Usuario = User.Identity.Name
                    };
                    _context.CajasValoradas.Add(cajaval);
                    await _context.SaveChangesAsync();

                    var salida = new
                    {
                        caja = cajaval,
                        documentos = new List<string>()
                    };
                    return Ok(salida);
                }
                else
                {

                    var documentos = from pasoval in 
                                            from mono in _context.PasosValijasValoradas 
                                            join credit in _context.Creditos on mono.FolioCredito equals credit.FolioCredito
                                            select new
                                            {
                                                mono.FolioCredito,
                                                mono.CodigoCajaValorada,
                                                mono.Usuario,
                                                TotalDocumentos = credit.TipoCredito == TipoCredito.Normal ? 2 : 1
                                            }
                                     where pasoval.CodigoCajaValorada == existe.CodigoSeguimiento && pasoval.Usuario == User.Identity.Name
                                     group pasoval by new { pasoval.FolioCredito, pasoval.TotalDocumentos } into slt
                                     //from exps in _context.ExpedientesCreditos.Include(ex => ex.Documentos).Include(ex => ex.Credito)
                                     //where exps.Credito.FolioCredito == slt.Key
                                     select new
                                     {
                                         Folio = slt.Key.FolioCredito,
                                         Pistoleados = slt.Count(),
                                         Total = slt.Key.TotalDocumentos
                                    };


                    var salida = new
                    {
                        caja = existe,
                        documentos = await documentos.OrderBy(d => d.Pistoleados).ToListAsync()
                    };
                    return Ok(salida);
                }
                
            }
            catch (Exception ex)
            {
                
                return BadRequest(ex.Message);
            }
        }

        [Route("despacho-a-custodia/caja-valorada/{codigoCaja}/agregar-documento/{folioDocumento}")]
        public async Task<IActionResult> AgregarDocumentoCajaValorada([FromRoute] string codigoCaja, [FromRoute] string folioDocumento)
        {
            try
            {
                /*Variables*/
                string codigoDocumento = folioDocumento.Substring(0, 2);
                string folioCredito = folioDocumento.Substring(2, 12);

                /*Obtener la caja valorada para agregar documento*/
                var cajaBuffer = _context.CajasValoradas.FirstOrDefault(cv => cv.CodigoSeguimiento == codigoCaja && cv.Usuario == User.Identity.Name && cv.MarcaAvance == "BUFFER");
                if (cajaBuffer == null)
                {
                    throw new Exception("Caja Valorada No existe en este contexto");
                }

                /*Valido quer no pistolee el mismo dos veces*/
                var existeDocumento = _context.PasosValijasValoradas.Any(pvv => pvv.FolioDocumento == folioDocumento);
                if (existeDocumento)
                {
                    throw new Exception("Documento ya Pistoleado");
                }

                var numeroTicket = _context.Creditos.FirstOrDefault(cre => cre.FolioCredito == folioCredito).NumeroTicket;
                var etapaDocumento = _wfService.OntenerTareaActual(ProcesoDocumentos.NOMBRE_PROCESO, numeroTicket);
                if(etapaDocumento.Etapa.NombreInterno != ProcesoDocumentos.ETAPA_DESPACHO_A_CUSTODIA)
                {
                    throw new Exception("Etapa de documento no corresponde");
                }


                //Se registra el nuevo folio dentro de la tabla de paso
                var registra = new PasoValijaValorada
                {
                    Id = Guid.NewGuid().ToString(),
                    CodigoCajaValorada = codigoCaja,
                    FolioCredito = folioCredito,
                    FolioDocumento = folioDocumento,
                    Usuario = User.Identity.Name
                };

                _context.PasosValijasValoradas.Add(registra);
                await _context.SaveChangesAsync();



                /* 
                
                var salida = from pasoval in _context.PasosValijasValoradas
                             where pasoval.CodigoCajaValorada == codigoCaja && pasoval.Usuario == User.Identity.Name
                             group pasoval by pasoval.FolioCredito into slt
                             from exps in _context.ExpedientesCreditos.Include(ex => ex.Documentos).Include(ex => ex.Credito)
                             where exps.Credito.FolioCredito == slt.Key
                             select new
                             {
                                 Folio = slt.Key,
                                 Pistoleados = slt.Count(),
                                 Total = exps.Documentos.Count
                             };

                    salida.ToList()


                
                */


                var documentos = from pasoval in
                                     from mono in _context.PasosValijasValoradas
                                     join credit in _context.Creditos on mono.FolioCredito equals credit.FolioCredito
                                     select new
                                     {
                                         mono.FolioCredito,
                                         mono.CodigoCajaValorada,
                                         mono.Usuario,
                                         TotalDocumentos = credit.TipoCredito == TipoCredito.Normal ? 2 : 1
                                     }
                                 where pasoval.CodigoCajaValorada == codigoCaja && pasoval.Usuario == User.Identity.Name
                                 group pasoval by new { pasoval.FolioCredito, pasoval.TotalDocumentos } into slt
                                 //from exps in _context.ExpedientesCreditos.Include(ex => ex.Documentos).Include(ex => ex.Credito)
                                 //where exps.Credito.FolioCredito == slt.Key
                                 select new
                                 {
                                     Folio = slt.Key.FolioCredito,
                                     Pistoleados = slt.Count(),
                                     Total = slt.Key.TotalDocumentos
                                 };

                var salida = await documentos.OrderBy(d => d.Pistoleados).ToListAsync();
                return Ok(salida);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("recepcion-custodia/{codigoSeguimiento}")]
        public async Task<IActionResult> RecepcionCustodia([FromBody] IEnumerable<ExpedienteGenerico> entrada, [FromRoute] string codigoSeguimiento)
        {
            var tran = _context.Database.BeginTransaction();
            try
            {
                List<string> ticketsAvanzar = new List<string>();
                foreach (var item in entrada)
                {
                    var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                    _wfService.AsignarVariable("EXPEDIENTE_FALTANTE_CUSTODIA", item.Faltante ? 1.ToString(): 0.ToString(), elExpediente.Credito.NumeroTicket);
                    ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
                }

                await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_RECEPCION_Y_CUSTODIA, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));
                var laCaja = _context.CajasValoradas.FirstOrDefault(v => v.CodigoSeguimiento == codigoSeguimiento);
                laCaja.MarcaAvance = "FN";
                _context.CajasValoradas.Update(laCaja);
                await _context.SaveChangesAsync();
                tran.Commit();
                return Ok();
            }
            catch (Exception)
            {
                tran.Rollback();
                return BadRequest();
            }
        }


        [HttpPost("preparar-nomina")]
        public async Task<IActionResult> PrepararNomina([FromBody] IEnumerable<ExpedienteGenerico> entrada)
        {
            var tran = _context.Database.BeginTransaction();
            try
            {
                List<string> ticketsAvanzar = new List<string>();
                foreach (var item in entrada)
                {
                    var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                    ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
                }
            
                await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_PREPARAR_NOMINA, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));
                await _context.SaveChangesAsync();
                tran.Commit();
                return Ok();
            }
            catch (Exception)
            {
                tran.Rollback();
                return BadRequest();
            }
        }


        [HttpPost("envio-a-notaria-rm")]
        public async Task<IActionResult> EnvioNotariaRM([FromBody] ColeccionExpedientesGenerica entrada)
        {
            var tran = _context.Database.BeginTransaction();
            try
            {
                List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
                List<string> ticketsAvanzar = new List<string>();
                var codificacionOficinaLogedIn = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
                var oficinaEnvio = _context.Oficinas.Include(ofi => ofi.Comuna).FirstOrDefault(d => d.Codificacion == codificacionOficinaLogedIn);
                var notariaEnvio = _context.Notarias.FirstOrDefault(d => d.Id == entrada.CodNotaria);

                DateTime now = DateTime.Now;
                var codSeg = now.Ticks.ToString() + "N" + notariaEnvio.Id.ToString().PadLeft(2, '0');


                var packNotaria = new PackNotaria
                {
                    FechaEnvio = DateTime.Now,
                    NotariaEnvio = notariaEnvio,
                    Oficina = oficinaEnvio,
                    CodigoSeguimiento = codSeg
                };

                _context.PacksNotarias.Add(packNotaria);

                foreach (var item in entrada.ExpedientesGenericos)
                {
                    var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                    elExpediente.PackNotaria = packNotaria;
                    expedientesModificados.Add(elExpediente);
                    ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
                }

                _context.ExpedientesCreditos.UpdateRange(expedientesModificados);
                
                await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_ENVIO_A_NOTARIA_RM, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));
                await _context.SaveChangesAsync();

                tran.Commit();
                return Ok(packNotaria);
            }
            catch (Exception)
            {
                tran.Rollback();
                return BadRequest();
            }
        }

        [HttpPost("recepciona-notaria-rm")]
        public async Task<IActionResult> RecepcionaNotariaRM([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            var tran = _context.Database.BeginTransaction();
            try
            {
                List<string> ticketsAvanzar = new List<string>();

                foreach (var item in entrada)
                {
                    var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                    ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
                }

                await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_RECEPCION_DE_NOTARIA_RM, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));
                await _context.SaveChangesAsync();
                tran.Commit();
                return Ok();
            }
            catch (Exception)
            {
                tran.Rollback();
                return BadRequest();
            }
        }

        [HttpPost("revision-documentos-rm")]
        public async Task<IActionResult> RevisionDocumentosRM([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            var tran = _context.Database.BeginTransaction();
            try
            {
                List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
                List<string> ticketsAvanzar = new List<string>();

                foreach (var item in entrada)
                {
                    var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                    _wfService.AsignarVariable("REPARO_REVISION_DOCUMENTO_LEGALIZADO", item.Reparo.ToString(), elExpediente.Credito.NumeroTicket);
                    ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
                }

                await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_REVISION_DOCUMENTOS_NOTARIA_RM, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));
                await _context.SaveChangesAsync();
                tran.Commit();
                return Ok(entrada);
            }
            catch (Exception)
            {
                tran.Rollback();
                return BadRequest();
            }
        }

        [HttpPost("despacho-reparo-notaria-rm")]
        public async Task<IActionResult> DespachoReparoNotariaRM([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            var tran = _context.Database.BeginTransaction();
            try
            {
                List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
                List<string> ticketsAvanzar = new List<string>();

                var codificacionOficinaLogedIn = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
                var oficinaEnvio = _context.Oficinas.Include(of => of.Comuna).FirstOrDefault(d => d.Codificacion == codificacionOficinaLogedIn);
                var notariaEnvio = _context.Notarias.Include(not => not.Comuna).FirstOrDefault(d => d.Comuna.Id == oficinaEnvio.Comuna.Id);


                DateTime now = DateTime.Now;
                var codSeg = now.Ticks.ToString() + "R" + notariaEnvio.Id.ToString().PadLeft(2, '0');

                var packNotaria = new PackNotaria
                {
                    FechaEnvio = DateTime.Now,
                    NotariaEnvio = notariaEnvio,
                    Oficina = oficinaEnvio,
                    CodigoSeguimiento = codSeg
                };

                _context.PacksNotarias.Add(packNotaria);

                foreach (var item in entrada)
                {
                    var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                    elExpediente.PackNotaria = packNotaria;
                    expedientesModificados.Add(elExpediente);
                    ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
                }

                _context.ExpedientesCreditos.UpdateRange(expedientesModificados);
                
                await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DEVOLUCION_REPARO_NOTARIA_RM, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));
                await _context.SaveChangesAsync();
                tran.Commit();
                return Ok(packNotaria);
            }
            catch (Exception)
            {
                tran.Rollback();
                return BadRequest();
            }
        }

        [HttpGet("listar-comerciales/{codigoCaja?}")]
        public async Task<IActionResult> ListarComerciales([FromRoute] string codigoCaja="")
        {
            
            //int cantidadExpedientesPorCaja = 80;
            var codificacionOficinaLogedIn = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
            //var laOficina = _context.Oficinas.Include(of => of.Comuna).FirstOrDefault(d => d.Codificacion == codificacionOficinaLogedIn);
            var comerciales = await _context.ExpedientesCreditos
                                .Include(exp => exp.AlmacenajeComercial)
                                .Include(exp => exp.Credito)
                                .Where(exp => (codigoCaja == String.Empty) ? exp.AlmacenajeComercial == null : exp.AlmacenajeComercial.CodigoSeguimiento == codigoCaja)
                                .Join(  _context.CargasIniciales, 
                                        expediente => expediente.Credito.FolioCredito, 
                                        cargainicial => cargainicial.FolioCredito,
                                        (expediente, cargainicial) => new {
                                            expediente, cargainicial
                                        }
                                ).Where(x => x.cargainicial.CodigoOficinaIngreso == codificacionOficinaLogedIn)
                                .OrderBy(ord => ord.expediente.Credito.FechaDesembolso).ThenBy(ord => ord.expediente.Credito.FolioCredito)
                                .ToListAsync();


            
            return Ok(comerciales);
        
        }

        [HttpGet("historial-cajas-comerciales")]
        public async Task<IActionResult> ListarHistorialCajas()
        {
            var codificacionOficinaLogedIn = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
            var retorno = await _context.AlmacenajesComerciales.Where(x => x.CodigoOficina == codificacionOficinaLogedIn).ToListAsync();
            return Ok(retorno);
        }

        [HttpPost("almacenaje-set-comercial")]
        public async Task<IActionResult> GuardarAlamacenajeComercial([FromBody] IEnumerable<ExpedienteGenerico> entrada)
        {
            var tran = _context.Database.BeginTransaction();
            try
            {
                var codificacionOficinaLogedIn = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
                AlmacenajeComercial almacen = new AlmacenajeComercial{
                    Id = Guid.NewGuid(),
                    CodigoSeguimiento = DateTime.Now.Ticks.ToString() + codificacionOficinaLogedIn,
                    Fecha = DateTime.Now,
                    CodigoOficina = codificacionOficinaLogedIn,
                    RutEjecutivo = User.Identity.Name
                };
                foreach(var item in entrada)
                {
                    var folio = item.FolioCredito;
                    var expediente = _context.ExpedientesCreditos.Include(exp => exp.Credito).FirstOrDefault(exp => exp.Credito.FolioCredito == folio);
                    almacen.Expedientes.Add(expediente);

                }
                _context.AlmacenajesComerciales.Add(almacen);
                await _context.SaveChangesAsync();
                tran.Commit();
                return Ok(almacen);
            }
            catch (Exception)
            {
                tran.Rollback();
                return BadRequest();
            }
        }


        [HttpPost("ingreso-expedientes-legalizados")]
        public async Task<IActionResult> IngresoExpedienteLelalizado([FromBody] IEnumerable<ExpedienteGenerico> entrada)
        {
            var tran = _context.Database.BeginTransaction();
            try
            {
                List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
                List<string> ticketsAvanzar = new List<string>();


                foreach (var item in entrada)
                {
                    var credito = await _context.Creditos.FirstOrDefaultAsync(xcred => xcred.FolioCredito == item.FolioCredito);
                    _wfService.AsignarVariable("LEGALIZADO_ANTES", 1.ToString(), credito.NumeroTicket);
                    ticketsAvanzar.Add(credito.NumeroTicket);
                }

                await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_ENVIO_NOTARIA, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));
                await _context.SaveChangesAsync();
                tran.Commit();
                return Ok();
            }
            catch (Exception)
            {
                tran.Rollback();
                return BadRequest();
            }
        }

        [HttpGet("reasignaciones/oficinas")]
        public IActionResult ConsultaExpedienteReasignacion([FromQuery] string q)
        {
            var fdata  = from cargas in _context.CargasIniciales
                        join comercial in _context.Oficinas on cargas.CodigoOficinaIngreso equals comercial.Codificacion
                        join legal in _context.Oficinas on cargas.CodigoOficinaPago equals legal.Codificacion
                        where cargas.FolioCredito == q 
                        select new {
                            CargaInicial = cargas,
                            OficinaComercial = comercial,
                            OficinaLegal = legal
                        };
            

            return Ok(fdata.FirstOrDefault());
        }

        [HttpPost("reasignaciones/oficinas")]
        public IActionResult GuardarExpedienteReasignacion([FromBody] dynamic entrada)
        {
            using(var tran = _context.Database.BeginTransaction())
            {


                try{
                    string folio = entrada.folioCredito.ToString();
                    int codOficinaReasignacion = Convert.ToInt32(entrada.nuevaOficina);
                    var cargaInicial = _context.CargasIniciales.FirstOrDefault(carga => carga.FolioCredito == folio);
                    var oficinaReasigna = _context.Oficinas.Include(f => f.OficinaProceso).FirstOrDefault(d => d.Id == codOficinaReasignacion);
                    var credito = _context.Creditos.FirstOrDefault(cred => cred.FolioCredito == folio);
                    string opOriginal = cargaInicial.CodigoOficinaPago;
                    var lasolicitud = _context.Solicitudes.Include(sol=>sol.Tareas).FirstOrDefault(sl => sl.NumeroTicket == credito.NumeroTicket);
                    var latarea = lasolicitud.Tareas.FirstOrDefault(tar => tar.Estado == EstadoTarea.Activada);
                    var laoforig = _context.Oficinas.Include(f => f.OficinaProceso).FirstOrDefault(of => of.Codificacion == opOriginal);


                    cargaInicial.CodigoOficinaPago = oficinaReasigna.Codificacion;
                    _wfService.AsignarVariable("OFICINA_PAGO", oficinaReasigna.Codificacion, credito.NumeroTicket);
                    _wfService.AsignarVariable("OFICINA_PROCESA_NOTARIA", oficinaReasigna.Codificacion, credito.NumeroTicket);

                    if(!laoforig.EsRM && oficinaReasigna.EsRM)
                    {
                        latarea.Etapa = _context.Etapas.FirstOrDefault(eta => eta.NombreInterno == ProcesoDocumentos.ETAPA_PREPARAR_NOMINA);
                        latarea.UnidadNegocioAsignada = oficinaReasigna.Codificacion;
                        _wfService.AsignarVariable("ES_RM", "1", credito.NumeroTicket);
                        _context.Tareas.Update(latarea);
                    }
                    else if(laoforig.EsRM && !oficinaReasigna.EsRM)
                    {
                        latarea.Etapa = _context.Etapas.FirstOrDefault(eta => eta.NombreInterno == ProcesoDocumentos.ETAPA_ENVIO_NOTARIA);
                        latarea.UnidadNegocioAsignada = oficinaReasigna.Codificacion;
                        _wfService.AsignarVariable("ES_RM", "0", credito.NumeroTicket);

                        _context.Tareas.Update(latarea);
                    }     
                    /*else if (oficinaReasigna.OficinaProceso.Id != oficinaReasigna.Id){

                    }*/

                    
                    _context.CargasIniciales.Update(cargaInicial);
                    AuditorReasignacion audit = new AuditorReasignacion
                    {
                        Id = Guid.NewGuid().ToString(),
                        UsuarioAccion = User.Identity.Name,
                        FechaAccion =  DateTime.Now,
                        TipoReasignacion = "Reasignacion Oficina Legal",
                        AsignacionOriginal = opOriginal,
                        AsignacionNueva = oficinaReasigna.Codificacion,
                        FolioCredito = credito.FolioCredito
                    };
                    _context.AudicionesReasignaciones.Add(audit);
                    _context.SaveChanges();
                    tran.Commit();
                    return Ok();
                }
                catch(Exception)
                {
                    tran.Rollback();
                    return BadRequest();
                }
            }
            
        }

    }
}
