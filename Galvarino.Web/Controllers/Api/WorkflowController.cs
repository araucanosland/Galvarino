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

        [HttpGet("obtener-expediente/{folioCredito}")]
        public IActionResult ObtenerXpediente([FromRoute] string folioCredito)
        {
            var expediente = _context.ExpedientesCreditos.Include(x => x.Credito).Include(x => x.Documentos).FirstOrDefault(x => x.Credito.FolioCredito == folioCredito);
            if (expediente.Documentos.Count() > 0)
            {
                return Ok(expediente);
            }
            else
            {
                return NotFound("No hay mano");
            }
        }

        [HttpGet("mis-solicitudes/{etapaIn?}")]
        public async Task<IActionResult> ListarMisSolicitudes(string etapaIn = ""){

            //var rolUsuario =  //FirstOrDefault(x => x.Type == ClaimTypes.Role).Value;    
            var oficinaUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;    
            var mistareas = _context.Tareas.Include(d => d.Etapa).Include(f => f.Solicitud).Where(x => (x.AsignadoA == User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\","") || x.Etapa.TipoUsuarioAsignado == TipoUsuarioAsignado.Rol && User.IsInRole(x.AsignadoA) && ((x.UnidadNegocioAsignada != null && x.UnidadNegocioAsignada == oficinaUsuario) || x.UnidadNegocioAsignada == null )) && x.Estado == EstadoTarea.Activada);

            if(!string.IsNullOrEmpty(etapaIn)){
                mistareas = mistareas.Where(g => g.Etapa.NombreInterno==etapaIn);
            }
            
            var salida = new List<dynamic>();
            await mistareas.ForEachAsync(tarea => {

                var folioCredito = _wfService.ObtenerVariable("FOLIO_CREDITO", tarea.Solicitud.NumeroTicket);
                var motivoDevol = _wfService.ObtenerVariable("CODIGO_MOTIVO_DEVOLUCION_A_SUCURSAL", tarea.Solicitud.NumeroTicket);
                var documentosFaltantes = _wfService.ObtenerVariable("COLECCION_DOCUMENTOS_FALTANTES", tarea.Solicitud.NumeroTicket);
                var credito = _context.Creditos.FirstOrDefault(cre => cre.FolioCredito == folioCredito);
                var expediente = _context.ExpedientesCreditos
                                .Include(f => f.Documentos)
                                .Include(f => f.Credito)
                                .Include(s => s.PackNotaria)
                                .Include(s => s.ValijaValorada)
                                .FirstOrDefault(ex => ex.Credito.FolioCredito == folioCredito);      

                salida.Add(new {
                    tarea = tarea,
                    credito = credito,
                    expediente = expediente,
                    reparo=motivoDevol.Length > 0 ? Convert.ToInt32(motivoDevol): 0,
                    documentosFaltantes
                });
            });
            
            
            return Ok(salida);
        }

        [HttpGet("solicitudes-reparo-devolicion/{oficina?}")]
        public async Task<IActionResult> ListarOficinasReparo(string oficina="")
        {

            var mistareas = _context.Tareas.Include(d => d.Etapa).Include(f => f.Solicitud).Where(x => x.AsignadoA == User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\","") && x.Estado == EstadoTarea.Activada && x.Etapa.NombreInterno == "DESPACHO_OF_PARTES_DEVOLUCION");

            var salida = new List<dynamic>();
            await mistareas.ForEachAsync(tarea =>
            {

                var folioCredito = _wfService.ObtenerVariable("FOLIO_CREDITO", tarea.Solicitud.NumeroTicket);
                var codigoOficinaDevolucion = _wfService.ObtenerVariable("OFINA_PROCESA_NOTARIA", tarea.Solicitud.NumeroTicket);

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
            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();

            var codificacionOficinaLogedIn = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
            var oficinaEnvio = _context.Oficinas.Include(fd => fd.OficinaProceso).FirstOrDefault(d => d.Codificacion == codificacionOficinaLogedIn);

            
            DateTime now = DateTime.Now;
            var codSeg = now.Ticks.ToString() + "O" + oficinaEnvio.Codificacion;

            var valijaMatrix = new ValijaOficina{
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
            await _context.SaveChangesAsync();
            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DESPACHO_OFICINA_NOTARIA, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));
            
            return Ok(valijaMatrix);
        }

        [HttpPost("recepcion-expedientes-matriz/{codigoSeguimiento}")]
        public async Task<IActionResult> RecepcionSetLegal([FromBody] IEnumerable<ExpedienteGenerico> entrada, [FromRoute] string codigoSeguimiento)
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
            return Ok();
        }

        [HttpPost("envio-a-notaria")]
        public async Task<IActionResult> EnvioNotaria([FromBody] ColeccionExpedientesGenerica entrada)
        {

            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();

            var codificacionOficinaLogedIn = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
            var oficinaEnvio = _context.Oficinas.Include(ofi => ofi.Comuna).FirstOrDefault(d => d.Codificacion == codificacionOficinaLogedIn);
            var notariaEnvio = _context.Notarias.FirstOrDefault(d => d.Id == entrada.CodNotaria);
                        
            DateTime now = DateTime.Now;
            var codSeg = now.Ticks.ToString() + "N" + notariaEnvio.Id.ToString().PadLeft(2,'0');
            
            
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
            await _context.SaveChangesAsync();
            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_ENVIO_NOTARIA, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));

            return Ok(packNotaria);
        }

        [HttpPost("recepciona-notaria")]
        public async Task<IActionResult> RecepcionaNotaria([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<string> ticketsAvanzar = new List<string>();

            foreach (var item in entrada)
            {
                var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
            }

            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_RECEPCION_NOTARIA, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));
            return Ok();
        }

        [HttpPost("revision-documentos")]
        public async Task<IActionResult> RevisionDocumentos([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
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
            return Ok(entrada);
        }

        [HttpPost("despacho-reparo-notaria")]
        public async Task<IActionResult> DespachoReparoNotaria([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();

            var codificacionOficinaLogedIn = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
            var oficinaEnvio = _context.Oficinas.FirstOrDefault(d => d.Codificacion == codificacionOficinaLogedIn);
            var notariaEnvio = _context.Notarias.FirstOrDefault(d => d.Comuna.Id == oficinaEnvio.Comuna.Id);

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
            await _context.SaveChangesAsync();
            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DESPACHO_REPARO_NOTARIA, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));
            return Ok(packNotaria);
        }

        [HttpPost("despacho-sucursal-oficiana-partes")]
        public async Task<IActionResult> DespachoSucOfPartes([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
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
                ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
            }

            _context.ExpedientesCreditos.UpdateRange(expedientesModificados);
            await _context.SaveChangesAsync();
            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DESPACHO_OFICINA_PARTES, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));
            return Ok(valijaEnvio);
        }

        [HttpGet("recepcion-oficina-partes/{codigoSeguimiento}")]
        public async Task<IActionResult> RecepcionOfPartes([FromRoute] string codigoSeguimiento)
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
            
            return Ok();
        }

        [HttpGet("recepcion-valija/{codigoSeguimiento}")]
        public async Task<IActionResult> RecepcionValija([FromRoute] string codigoSeguimiento)
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

            return Ok();
        }


        [HttpPost("apertura-valija/{codigoSeguimiento}")]
        public async Task<IActionResult> AperturaValija([FromBody] IEnumerable<ExpedienteGenerico> entrada, [FromRoute] string codigoSeguimiento)
        {
            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();


            foreach (var item in entrada)
            {
                var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).Include(d => d.Documentos).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                _wfService.AsignarVariable("EXPEDIENTE_FALTANTE", item.Faltante ? "1" : "0", elExpediente.Credito.NumeroTicket);
                
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
            
            return Ok();
        }

        [Route("analisis-mesa-control")]
        public async Task<IActionResult> AnalisisMesaControl([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<string> ticketsAvanzar = new List<string>();
            List<EnvioNotariaFormHelper> expedientesReparo = new List<EnvioNotariaFormHelper>();
            var opt = new string[] { "Sin Reparos", "Sin Firma de Notario", "Sin Timbre de Notario", "Sin Firma ni Timbre", "Ilegible" };
            foreach (var item in entrada)
            {
                var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                _wfService.AsignarVariable("DEVOLUCION_A_SUCURSAL", item.Reparo > 0 ? "1":"0", elExpediente.Credito.NumeroTicket);
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
            return Ok();
        }

        [Route("solucion-expediente-faltante")]
        public async Task<IActionResult> SolucionExpedienteFaltante([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<string> ticketsAvanzar = new List<string>();

            foreach (var item in entrada)
            {
                var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                _wfService.AsignarVariable("DOCUMENTACION_FALTANTE_EN_TRANSITO", "1", elExpediente.Credito.NumeroTicket);
                ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
            }
            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DOCUMENTACION_FALTANTE, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));
            return Ok();
        }

        [Route("despacho-reparo-oficina-partes")]
        public async Task<IActionResult> DespachoOfPtReparoSucursal([FromBody] ColeccionExpedientesGenerica entrada)
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
            await _context.SaveChangesAsync();
            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DESPACHO_OF_PARTES_DEVOLUCION, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));
            return Ok(valijaEnvio);
        }

        [HttpGet("despacho-oficina-correccion/{codigoSeguimiento}")]
        public async Task<IActionResult> DespachoOfCorreccion([FromRoute] string codigoSeguimiento)
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

            return Ok();
        }

        [HttpPost("solucion-reparo-sucursal")]
        public async Task<IActionResult> SolucionReparosSucursal([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<string> ticketsAvanzar = new List<string>();
           foreach (var item in entrada)
            {
                var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
            }

            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_SOLUCION_REPAROS_SUCURSAL, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));
            return Ok();
        }

        [Route("despacho-a-custodia")]
        public async Task<IActionResult> DespachoCustodia([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();


            DateTime now = DateTime.Now;
            var codSeg = now.Ticks.ToString() + "CV" ;

            CajaValorada cajaval = new CajaValorada{
                FechaEnvio = DateTime.Now,
                CodigoSeguimiento = codSeg,
                MarcaAvance = "DESPACUST"
            };
            _context.CajasValoradas.Add(cajaval);

            foreach (var item in entrada)
            {
                var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                elExpediente.CajaValorada = cajaval;
                expedientesModificados.Add(elExpediente);
                ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
                //_wfService.AsignarVariable("FOLIO_CAJA_VALORADA", cajaval.CodigoSeguimiento, elExpediente.Credito.NumeroTicket);
            }

            
            _context.ExpedientesCreditos.UpdateRange(expedientesModificados);
            await _context.SaveChangesAsync();
            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DESPACHO_A_CUSTODIA, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));
            return Ok(cajaval);
        }


        [HttpPost("recepcion-custodia/{codigoSeguimiento}")]
        public async Task<IActionResult> RecepcionCustodia([FromBody] IEnumerable<ExpedienteGenerico> entrada, [FromRoute] string codigoSeguimiento)
        {
            List<string> ticketsAvanzar = new List<string>();
            foreach (var item in entrada)
            {
                var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                _wfService.AsignarVariable("EXPEDIENTE_FALTANTE_CUSTODIA", item.Faltante ? "1": "0", elExpediente.Credito.NumeroTicket);
                ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
            }

            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_RECEPCION_Y_CUSTODIA, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));
            var laCaja = _context.CajasValoradas.FirstOrDefault(v => v.CodigoSeguimiento == codigoSeguimiento);
            laCaja.MarcaAvance = "FN";
            _context.CajasValoradas.Update(laCaja);
            await _context.SaveChangesAsync();
            
            return Ok();
        }
    }
}
