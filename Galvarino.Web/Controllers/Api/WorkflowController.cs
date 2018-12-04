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
            var oficinaUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
            var ofipago = _wfService.ObtenerVariable("OFICINA_PAGO", expediente.Credito.NumeroTicket);

            if (expediente.Documentos.Count() > 0 && ofipago == oficinaUsuario)
            {
                return Ok(expediente);
            }
            else
            {
                var ofiObject = _context.Oficinas.FirstOrDefault(of => of.Codificacion == ofipago);
                return NotFound("Carga del Expediente en: " + ofiObject.Nombre);
            }
        }

        [HttpGet("mis-solicitudes/{etapaIn?}")]
        public async Task<IActionResult> ListarMisSolicitudes(string etapaIn = ""){

            //var rolUsuario =  //FirstOrDefault(x => x.Type == ClaimTypes.Role).Value;    
            var oficinaUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value; 
            var ObjetoOficinaUsuario = _context.Oficinas.Include(of=> of.OficinaProceso).FirstOrDefault(ofc => ofc.Codificacion == oficinaUsuario);
            var OficinasUsuario = _context.Oficinas.Where(ofc => ofc.OficinaProceso.Id == ObjetoOficinaUsuario.Id && ofc.EsMovil == true);
            var ofinales = new List<Oficina>();
            ofinales.Add(ObjetoOficinaUsuario);
            ofinales.AddRange(OficinasUsuario.ToList());

            var mistareas = _context.Tareas.Include(d => d.Etapa).Include(f => f.Solicitud).Where(x => (x.AsignadoA == User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\","") || x.Etapa.TipoUsuarioAsignado == TipoUsuarioAsignado.Rol && User.IsInRole(x.AsignadoA) && ((x.UnidadNegocioAsignada != null && ofinales.Select(ofs => ofs.Codificacion).Contains(x.UnidadNegocioAsignada)) || x.UnidadNegocioAsignada == null )) && x.Estado == EstadoTarea.Activada);

            if(!string.IsNullOrEmpty(etapaIn)){
                mistareas = mistareas.Where(g => g.Etapa.NombreInterno==etapaIn);
            }
            
            var salida = new List<dynamic>();
            await mistareas.ForEachAsync(tarea => {

                var folioCredito = _wfService.ObtenerVariable("FOLIO_CREDITO", tarea.Solicitud.NumeroTicket);
                var motivoDevol = _wfService.ObtenerVariable("CODIGO_MOTIVO_DEVOLUCION_A_SUCURSAL", tarea.Solicitud.NumeroTicket);
                var reparoNotaria = _wfService.ObtenerVariable("REPARO_REVISION_DOCUMENTO_LEGALIZADO", tarea.Solicitud.NumeroTicket);
                var documentosFaltantes = _wfService.ObtenerVariable("COLECCION_DOCUMENTOS_FALTANTES", tarea.Solicitud.NumeroTicket);
                var credito = _context.Creditos.FirstOrDefault(cre => cre.FolioCredito == folioCredito);
                var expediente = _context.ExpedientesCreditos
                                .Include(f => f.Documentos)
                                .Include(f => f.Credito)
                                .Include(s => s.PackNotaria)
                                .Include(s => s.ValijaValorada)
                                .FirstOrDefault(ex => ex.Credito.FolioCredito == folioCredito);      

                expediente.Documentos = expediente.Documentos.OrderBy(r => r.Codificacion).ToList();


                salida.Add(new {
                    tarea = tarea,
                    credito = credito,
                    expediente = expediente,
                    reparo=motivoDevol.Length > 0 ? Convert.ToInt32(motivoDevol): 0,
                    reparoNotaria = reparoNotaria,
                    documentosFaltantes
                });
            });
            
            
            return Ok(salida);
        }

        [HttpGet("mi-resumen")]
        public IActionResult MiResumen()
        {
            var salida = new List<dynamic>();
            //User.IsInRole("")
            var oficinaUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
            var oficinas = _context.Oficinas.Include(d => d.OficinaProceso).Where(ofi => ofi.Id == ofi.OficinaProceso.Id && ofi.Codificacion == oficinaUsuario);
            var tareas = _context.Tareas.Include(d => d.Etapa).Include(f => f.Solicitud).Where(x => x.Estado == EstadoTarea.Activada);
            var expedientes = _context.ExpedientesCreditos
                                .Include(f => f.Documentos)
                                .Include(f => f.Credito);

            var offices = from off in oficinas
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
        public async Task<IActionResult> ListarMisSolicitudesNotaria(string etapaIn, int notaria)
        {

            var oficinaUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
            var ObjetoOficinaUsuario = _context.Oficinas.Include(of => of.OficinaProceso).FirstOrDefault(ofc => ofc.OficinaProceso.Codificacion == oficinaUsuario);
            var OficinasUsuario = _context.Oficinas.Where(ofc => ofc.OficinaProceso.Id == ObjetoOficinaUsuario.Id);

            var mistareas = _context.Tareas.Include(d => d.Etapa).Include(f => f.Solicitud).Where(x => (x.AsignadoA == User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", "") || x.Etapa.TipoUsuarioAsignado == TipoUsuarioAsignado.Rol && User.IsInRole(x.AsignadoA) && ((x.UnidadNegocioAsignada != null && OficinasUsuario.Select(ofs => ofs.Codificacion).Contains(x.UnidadNegocioAsignada)) || x.UnidadNegocioAsignada == null)) && x.Estado == EstadoTarea.Activada && x.Etapa.NombreInterno == etapaIn);

            
            var salida = new List<dynamic>();
            //await mistareas.ForEachAsync(tarea =>
            foreach(Tarea tarea in mistareas)
            {

                var folioCredito = _wfService.ObtenerVariable("FOLIO_CREDITO", tarea.Solicitud.NumeroTicket);
                var motivoDevol = _wfService.ObtenerVariable("CODIGO_MOTIVO_DEVOLUCION_A_SUCURSAL", tarea.Solicitud.NumeroTicket);
                var reparoNotaria = _wfService.ObtenerVariable("REPARO_REVISION_DOCUMENTO_LEGALIZADO", tarea.Solicitud.NumeroTicket);
                var documentosFaltantes = _wfService.ObtenerVariable("COLECCION_DOCUMENTOS_FALTANTES", tarea.Solicitud.NumeroTicket);
                var credito = _context.Creditos.FirstOrDefault(cre => cre.FolioCredito == folioCredito);
                var expediente = _context.ExpedientesCreditos
                                .Include(f => f.Documentos)
                                .Include(f => f.Credito)
                                .Include(s => s.PackNotaria)
                                .ThenInclude(pn => pn.NotariaEnvio)
                                .Include(s => s.ValijaValorada)
                                .FirstOrDefault(ex => ex.Credito.FolioCredito == folioCredito && ex.PackNotaria.NotariaEnvio.Id == notaria);



                if(expediente != null){
                    expediente.Documentos = expediente.Documentos.OrderBy(r => r.Codificacion).ToList();
                    salida.Add(new
                    {
                        tarea = tarea,
                        credito = credito,
                        expediente = expediente,
                        reparo = motivoDevol.Length > 0 ? Convert.ToInt32(motivoDevol) : 0,
                        reparoNotaria = reparoNotaria,
                        documentosFaltantes
                    });
                }
                
            }
            //);


            return Ok(salida);
        }

        [HttpGet("documentos-generados/{tipoDocumento}")]
        public async Task<IActionResult> ListarMisDocumentosGenerados(string tipoDocumento)
        {

            var oficinaUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
            var ObjetoOficinaUsuario = await _context.Oficinas.Include(of => of.OficinaProceso).Include(of => of.PacksNotaria).ThenInclude(pn => pn.Expedientes).FirstOrDefaultAsync(ofc => ofc.OficinaProceso.Codificacion == oficinaUsuario);
            var OficinasUsuario = _context.Oficinas.Where(ofc => ofc.OficinaProceso.Id == ObjetoOficinaUsuario.Id);

            switch(tipoDocumento)
            {
                case "nomina-notaria":
                    return Ok(ObjetoOficinaUsuario.PacksNotaria);
                
                case "valija-oficina":
                    var valijaOficina = await _context.ValijasOficinas.Include(d => d.OficinaEnvio).Include(d => d.Expedientes).Where(x => x.OficinaEnvio.Codificacion == oficinaUsuario).ToListAsync();
                    return Ok(valijaOficina);

                case "valija-valorada":
                    var valijaValodara = await _context.ValijasValoradas.Include(d => d.Oficina).Include(d => d.Expedientes).Where(x => x.Oficina.Codificacion == oficinaUsuario).ToListAsync();
                    return Ok(valijaValodara);
                
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
                _wfService.AsignarVariable("USUARIO_DESPACHA_A_OF_PARTES", User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""), elExpediente.Credito.NumeroTicket);
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
            return Ok();
        }

        [Route("solucion-expediente-faltante")]
        public async Task<IActionResult> SolucionExpedienteFaltante([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<string> ticketsAvanzar = new List<string>();

            foreach (var item in entrada)
            {
                var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                _wfService.AsignarVariable("DOCUMENTACION_FALTANTE_EN_TRANSITO", 1.ToString(), elExpediente.Credito.NumeroTicket);
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
                _wfService.AsignarVariable("EXPEDIENTE_FALTANTE_CUSTODIA", item.Faltante ? 1.ToString(): 0.ToString(), elExpediente.Credito.NumeroTicket);
                ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
            }

            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_RECEPCION_Y_CUSTODIA, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\",""));
            var laCaja = _context.CajasValoradas.FirstOrDefault(v => v.CodigoSeguimiento == codigoSeguimiento);
            laCaja.MarcaAvance = "FN";
            _context.CajasValoradas.Update(laCaja);
            await _context.SaveChangesAsync();
            
            return Ok();
        }


        [HttpPost("preparar-nomina")]
        public async Task<IActionResult> PrepararNomina([FromBody] IEnumerable<ExpedienteGenerico> entrada)
        {
            List<string> ticketsAvanzar = new List<string>();
            foreach (var item in entrada)
            {
                var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
            }
            
            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_PREPARAR_NOMINA, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));

            return Ok();
        }


        [HttpPost("envio-a-notaria-rm")]
        public async Task<IActionResult> EnvioNotariaRM([FromBody] ColeccionExpedientesGenerica entrada)
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
            await _context.SaveChangesAsync();
            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_ENVIO_A_NOTARIA_RM, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));

            return Ok(packNotaria);
        }

        [HttpPost("recepciona-notaria-rm")]
        public async Task<IActionResult> RecepcionaNotariaRM([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<string> ticketsAvanzar = new List<string>();

            foreach (var item in entrada)
            {
                var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
            }

            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_RECEPCION_DE_NOTARIA_RM, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));
            return Ok();
        }

        [HttpPost("revision-documentos-rm")]
        public async Task<IActionResult> RevisionDocumentosRM([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
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
            return Ok(entrada);
        }

        [HttpPost("despacho-reparo-notaria-rm")]
        public async Task<IActionResult> DespachoReparoNotariaRM([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
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
            await _context.SaveChangesAsync();
            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DEVOLUCION_REPARO_NOTARIA_RM, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));
            return Ok(packNotaria);
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
                                .OrderByDescending(ord => ord.expediente.Credito.FechaDesembolso).ThenByDescending(ord => ord.expediente.Credito.FolioCredito)
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
        public async Task<IActionResult> GuardarAlamacenajeComercial([FromBody] IEnumerable<ExpedienteGenerico> entrada){

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
            
            return Ok();
        }


        [HttpPost("ingreso-expedientes-legalizados")]
        public async Task<IActionResult> IngresoExpedienteLelalizado([FromBody] IEnumerable<ExpedienteGenerico> entrada)
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

            return Ok();
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
