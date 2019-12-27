using Galvarino.Web.Data;
using Galvarino.Web.Data.Repository;
using Galvarino.Web.Models.Application;
using Galvarino.Web.Models.Helper;
using Galvarino.Web.Models.Security;
using Galvarino.Web.Models.Workflow;
using Galvarino.Web.Services.Notification;
using Galvarino.Web.Services.Workflow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace Galvarino.Web.Controllers.Api
{
    [Route("api/wfSc/v1")]
    [ApiController]

    public class WorkflowScController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWorkflowService _wfService;
        private readonly INotificationKernel _mailService;
        private readonly ISolicitudRepository _solicitudRepository;
        public WorkflowScController(ApplicationDbContext context, IWorkflowService wfservice, INotificationKernel mailService, ISolicitudRepository solicitudRepo)
        {
            _context = context;
            _wfService = wfservice;
            _mailService = mailService;
            _solicitudRepository = solicitudRepo;
        }



        [HttpGet("recepcion-valija-sc/{codigoSeguimiento}")]
        public async Task<IActionResult> RecepcionValija([FromRoute] string codigoSeguimiento)
        {

            List<string> ticketsAvanzar = new List<string>();
            var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).Include(d => d.ValijaValorada).Where(x => x.ValijaValorada.CodigoSeguimiento == codigoSeguimiento && x.TipoExpediente == TipoExpediente.Complementario);

            foreach (var item in elExpediente)
            {
                var expedienteComplementario = _context.ExpedientesComplementarios.Where(x => x.CreditoId == item.Credito.Id).FirstOrDefault();
                ticketsAvanzar.Add(expedienteComplementario.NumeroTicket);
            }

            var laValija = _context.ValijasValoradas.FirstOrDefault(v => v.CodigoSeguimiento == codigoSeguimiento);
            laValija.MarcaAvance = "MSC_APERTURA";
            _context.ValijasValoradas.Update(laValija);
            await _context.SaveChangesAsync();

            await _wfService.AvanzarSetComplementario(ProcesoDocumentos.ETAPA_NOMINA_SET_COMPLEMENTARIO, ProcesoDocumentos.ETAPA_RECEPCIÓN_VALIJA_MESA_CONTROL_SET_COMPLEMENTARIO, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));


            return Ok();
        }





        [HttpGet("recepcion-valija-documentos-credito-sc/{codigoSeguimiento}")]
        public async Task<IActionResult> RecepcionValijadocumentocreditoSc([FromRoute] string codigoSeguimiento)
        {

            List<string> ticketsAvanzar = new List<string>();
          
            //var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).Include(d => d.ValijaValorada).Where(x => x.ValijaValorada.CodigoSeguimiento == codigoSeguimiento && x.TipoExpediente == TipoExpediente.Complementario);

            var elExpediente = _context.ExpedientesComplementarios.Where(d => d.CodigoSeguimiento == codigoSeguimiento).ToList();


            foreach (var item in elExpediente)
            {
                // var expedienteComplementario = _context.ExpedientesComplementarios.Where(x => x.FolioCredito == item.Credito.FolioCredito).FirstOrDefault();
                ticketsAvanzar.Add(item.NumeroTicket);
            }


            var laValija = _context.ValijasValoradas.FirstOrDefault(v => v.CodigoSeguimiento == codigoSeguimiento);
            laValija.MarcaAvance = "OFICINA_EVALUADORA_APERTURA";
            _context.ValijasValoradas.Update(laValija);
            await _context.SaveChangesAsync();

            await _wfService.AvanzarSetComplementario(ProcesoDocumentos.ETAPA_NOMINA_SET_COMPLEMENTARIO, ProcesoDocumentos.ETAPA_RECEPCION_VALIJA_DOCUMENTOS_OFICINA_DESTINO, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));


            return Ok();
        }





        [HttpGet("recepcion-oficina-partes-sc/{codigoSeguimiento}")]
        public async Task<IActionResult> RecepcionOfPartes([FromRoute] string codigoSeguimiento)
        {
            List<string> ticketsAvanzar = new List<string>();

            var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).Include(d => d.ValijaValorada).Where(x => x.ValijaValorada.CodigoSeguimiento == codigoSeguimiento && x.TipoExpediente == TipoExpediente.Complementario);

            //var elExpediente = _context.ExpedientesComplementarios.Where(d => d.FolioCredito == item.FolioCredito).FirstOrDefault();

            foreach (var item in elExpediente)
            {
                var expedienteComplementario = _context.ExpedientesComplementarios.Where(x => x.CreditoId == item.Credito.Id).FirstOrDefault();
                ticketsAvanzar.Add(expedienteComplementario.NumeroTicket);
            }

            var laValija = _context.ValijasValoradas.FirstOrDefault(v => v.CodigoSeguimiento == codigoSeguimiento);
            laValija.MarcaAvance = "MS_CONTROL";
            _context.ValijasValoradas.Update(laValija);
            await _context.SaveChangesAsync();

            await _wfService.AvanzarSetComplementario(ProcesoDocumentos.ETAPA_NOMINA_SET_COMPLEMENTARIO, ProcesoDocumentos.ETAPA_RECEPCIÓN_OFICINA_DE_PARTES_SET_COMPLEMENTARIO, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));

            return Ok();
        }


        [HttpGet("despacho-oficina-Ingreso-sc/{codigoSeguimiento}")]
        public async Task<IActionResult> despachOfIngreso([FromRoute] string codigoSeguimiento)
        {
            List<string> ticketsAvanzar = new List<string>();

            //var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).Include(d => d.ValijaValorada).Where(x => x.ValijaValorada.CodigoSeguimiento == codigoSeguimiento && x.TipoExpediente == TipoExpediente.Complementario);

            var elExpediente = _context.ExpedientesComplementarios.Where(d => d.CodigoSeguimiento == codigoSeguimiento).ToList();
            

            foreach (var item in elExpediente)
            {
               // var expedienteComplementario = _context.ExpedientesComplementarios.Where(x => x.FolioCredito == item.Credito.FolioCredito).FirstOrDefault();
                ticketsAvanzar.Add(item.NumeroTicket);
            }

            var laValija = _context.ValijasValoradas.FirstOrDefault(v => v.CodigoSeguimiento == codigoSeguimiento);
            laValija.MarcaAvance = "OFICINA_DESPACHO_OF_EVALUADORA";
            _context.ValijasValoradas.Update(laValija);
            await _context.SaveChangesAsync();

            await _wfService.AvanzarSetComplementario(ProcesoDocumentos.ETAPA_NOMINA_SET_COMPLEMENTARIO, ProcesoDocumentos.ETAPA_DESPACHO_DOCUMENTO_OFICINA_DESTINO, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));

            return Ok();
        }


        [Route("despacho-a-custodia-sc/{codigoCaja}")]
        public async Task<IActionResult> DespachoCustodiaSc([FromRoute] string codigoCaja)
        {

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
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("analisis-mesa-control-sc")]
        public async Task<IActionResult> AnalisisMesaControlSc([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {

            try
            {
                List<string> ticketsAvanzar = new List<string>();
                List<string> ticketsAvanzarReparo = new List<string>();
                List<string> seguimiento = new List<string>();
                List<EnvioNotariaFormHelper> expedientesReparo = new List<EnvioNotariaFormHelper>();
                var opt = new string[] { "Sin Reparos", "Falta Documento" };
                foreach (var item in entrada)
                {
                    var ExpedienteCred = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito && x.TipoExpediente == TipoExpediente.Complementario);
                    var elExpediente = _context.ExpedientesComplementarios.Where(a => a.CreditoId == ExpedienteCred.CreditoId).FirstOrDefault();


                    _wfService.AsignarVariable("DEVOLUCION_A_SUCURSAL_PAGO_SET_COMPLEMENTARIOS", item.Reparo > 0 ? 1.ToString() : 0.ToString(), elExpediente.NumeroTicket);
                    if (item.Reparo > 0)
                    {
                        _wfService.AsignarVariable("CODIGO_MOTIVO_DEVOLUCION_A_SUCURSAL_PAGO_SET_COMPLEMENTARIOS", item.Reparo.ToString(), elExpediente.NumeroTicket);
                        _wfService.AsignarVariable("TEXTO_MOTIVO_DEVOLUCION_A_SUCURSAL_PAGO_SET_COMPLEMENTARIOS", opt[item.Reparo], elExpediente.NumeroTicket);
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
                        var enviarA = _wfService.QuienCerroEtapaSetComplementario(ProcesoDocumentos.ETAPA_NOMINA_SET_COMPLEMENTARIO, ProcesoDocumentos.ETAPA_APERTURA_VALIJA_SET_COMPLEMENTARIO, elExpediente.NumeroTicket);
                        await _mailService.SendEmail(enviarA.NormalizedEmail, "Notificación de Expediente con Reparos: " + item.FolioCredito, mensaje);

                        ticketsAvanzarReparo.Add(elExpediente.NumeroTicket);
                        _wfService.ForzarAvance(ProcesoDocumentos.ETAPA_NOMINA_SET_COMPLEMENTARIO, ProcesoDocumentos.ETAPA_SOLUCION_REPAROS_SUCURSAL_PAGADORA, ticketsAvanzarReparo, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));

                    }
                    else
                    {
                        ticketsAvanzar.Add(elExpediente.NumeroTicket);

                    }

                       
                }

                await _wfService.AvanzarSetComplementario(ProcesoDocumentos.ETAPA_NOMINA_SET_COMPLEMENTARIO, ProcesoDocumentos.ETAPA_ANALISIS_MESA_CONTROL_SET_COMPLEMETARIOS, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }


      

        [Route("despacho-a-custodia-sc/chequear-caja-valorada")]
        public IActionResult ChequearCajaValoradaSc()
        {
            try
            {
                var existe = _context.CajasValoradas.FirstOrDefault(ca => ca.Usuario == "09217315-1" && ca.MarcaAvance == "BUFFER");
                return Ok(new { status = existe != null });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [Route("despacho-a-custodia-sc/generar-caja-valorada/{skp?}")]
        public async Task<IActionResult> GenerarCajaValoradaSc([FromRoute] string skp = "")
        {
            try
            {
                var existe = _context.CajasValoradas.FirstOrDefault(ca => ca.Usuario == User.Identity.Name && ca.MarcaAvance == "BUFFER");
                if (existe == null)
                {
                    CajaValorada cajaval = new CajaValorada
                    {
                        FechaEnvio = DateTime.Now,
                        CodigoSeguimiento = skp,
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

                    //var documentos = from pasoval in
                    //                     from mono in _context.PasosValijasValoradas
                    //                     join credit in _context.Creditos on mono.FolioCredito equals credit.FolioCredito
                    //                     select new
                    //                     {
                    //                         mono.FolioCredito,
                    //                         mono.CodigoCajaValorada,
                    //                         mono.Usuario,
                    //                         TotalDocumentos = credit.TipoCredito == TipoCredito.Normal ? 2 : 1
                    //                     }
                    //                 where pasoval.CodigoCajaValorada == existe.CodigoSeguimiento && pasoval.Usuario == User.Identity.Name
                    //                 group pasoval by new { pasoval.FolioCredito, pasoval.TotalDocumentos } into slt
                    //                 //from exps in _context.ExpedientesCreditos.Include(ex => ex.Documentos).Include(ex => ex.Credito)
                    //                 //where exps.Credito.FolioCredito == slt.Key
                    //                 select new
                    //                 {
                    //                     Folio = slt.Key.FolioCredito,
                    //                     Pistoleados = slt.Count(),
                    //                     Total = slt.Key.TotalDocumentos
                    //                 };

                    var documentos = _solicitudRepository.listarSegurosPorValijas(skp, User.Identity.Name);
                    var salida = new
                    {
                        caja = existe,
                        documentos =  documentos.OrderBy(d => d.Pistoleados).ToList()
                    };
                    return Ok(salida);
                }

            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }


        [HttpGet("mis-solicitudes-mesa-control-sc/{etapaIn?}")]
        public async Task<IActionResult> ListarMisSolicitudesMSC([FromRoute] string etapaIn = "", [FromQuery] int offset = 0, [FromQuery] int limit = 20)
        {
            try
            {
               

                //var rolUsuario =  //FirstOrDefault(x => x.Type == ClaimTypes.Role).Value;    
                var oficinaUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
                var ObjetoOficinaUsuario = _context.Oficinas.Include(of => of.OficinaProceso).FirstOrDefault(ofc => ofc.Codificacion == oficinaUsuario);
                var OficinasUsuario = _context.Oficinas.Where(ofc => ofc.OficinaProceso.Id == ObjetoOficinaUsuario.Id && ofc.EsMovil == true);
                var ofinales = new List<Oficina>();
                ofinales.Add(ObjetoOficinaUsuario);
                ofinales.AddRange(OficinasUsuario.ToList());

               
                User.IsInRole("Mesa Control");
                _context.Database.SetCommandTimeout(300);

                /*Optimizando */
                var laSalida = from tarea in _context.Tareas
                               join etapa in _context.Etapas on tarea.Etapa equals etapa
                               join solicitud in _context.Solicitudes on tarea.Solicitud equals solicitud
                               join ExpedienteComplementario in _context.ExpedientesComplementarios on solicitud.NumeroTicket equals ExpedienteComplementario.NumeroTicket
                               join credito in _context.Creditos on ExpedienteComplementario.CreditoId equals credito.Id
                               join expediente in _context.ExpedientesCreditos.Include(exp => exp.Documentos) on credito equals expediente.Credito

                               where (
                                             //  credito.FolioCredito=="093000419104"
                                                ((tarea.AsignadoA == User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", "") || etapa.TipoUsuarioAsignado == TipoUsuarioAsignado.Rol && User.IsInRole(tarea.AsignadoA)))
                                               && (((tarea.UnidadNegocioAsignada != null && ofinales.Select(ofs => ofs.Codificacion).Contains(tarea.UnidadNegocioAsignada)) || tarea.UnidadNegocioAsignada == null))
                                               && expediente.TipoExpediente == TipoExpediente.Complementario
                                              && solicitud.Proceso.Id == 2
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
            catch (Exception ex)
            {

                throw ex;
            }
        }


        [HttpGet("listar-expedientes-valija-sc/{folioValija}")]
        public IActionResult ListarExpedientesValija(string folioValija)
        {
            var expedientes = _context.ExpedientesCreditos.Include(e => e.Documentos).Include(e => e.Credito).Where(d => d.ValijaValorada.CodigoSeguimiento == folioValija && d.TipoExpediente == TipoExpediente.Complementario);
            return Ok(expedientes);
        }


        [HttpGet("listar-expedientes-valija-Documentos-sc/{folioValija}")]
        public IActionResult ListarExpedientesDocumentosValijaSc(string folioValija)
        {

            var expcomp = new List<ExpedienteComplementario>();
            var valorada = new List<ValijaValorada>();
            var creditos = new List<Credito>();
            var excred = new List<ExpedienteCredito>();
            List<Documento> docu = new List<Documento>();
            expcomp = _context.ExpedientesComplementarios.Where(a => a.CodigoSeguimiento == folioValija).ToList();

            foreach (var item in expcomp)
            {
                creditos.Add( _context.Creditos.Where(a => a.Id == item.CreditoId).FirstOrDefault());
                //valorada.Add(_context.ValijasValoradas.Where(a => a.CodigoSeguimiento == item.CodigoSeguimiento && a.CodigoSeguimiento == folioValija && a.MarcaAvance == "OF_PARTES_DOCUMENTOS").FirstOrDefault());

            }

            foreach (var item in creditos)
            {
                excred.Add(_context.ExpedientesCreditos.Where(a => a.CreditoId == item.Id).FirstOrDefault());
            }
             foreach (var item in excred)
            {

                var doc = _context.Documentos.Where(a => a.ExpedienteCredito.CreditoId == item.CreditoId && a.Codificacion!="10" && a.Codificacion!="09" ).ToList();
               
            }


            //var expedientes = _context.ExpedientesCreditos.Include(e => e.Documentos).Include(e => e.Credito).Where(d => d.ValijaValorada.CodigoSeguimiento == folioValija && d.TipoExpediente == TipoExpediente.Complementario);
            return Ok(excred);
        }

        [HttpPost("apertura-valija-Docmentos-creditosc/{codigoSeguimiento}")]
        public async Task<IActionResult> AperturaValijaDocumentosSc([FromBody] IEnumerable<ExpedienteGenerico> entrada, [FromRoute] string codigoSeguimiento)
        {

            try
            {
                List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
                List<string> ticketsAvanzar = new List<string>();

                var elExpedienteApertura = _context.ExpedientesComplementarios.Where(d => d.CodigoSeguimiento == codigoSeguimiento).ToList();

                foreach (var item in elExpedienteApertura)
                {


                    ticketsAvanzar.Add(item.NumeroTicket);
                }

                var laValija = _context.ValijasValoradas.FirstOrDefault(v => v.CodigoSeguimiento == codigoSeguimiento);
                laValija.MarcaAvance = "FN";
                _context.ValijasValoradas.Update(laValija);
                await _context.SaveChangesAsync();

                await _wfService.AvanzarSetComplementario(ProcesoDocumentos.ETAPA_NOMINA_SET_COMPLEMENTARIO, ProcesoDocumentos.ETAPA_APERTURA_VALIJA_DOCUMENTOS_OFICINA_DESTINO, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));


                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }



        [HttpPost("apertura-valija-sc/{codigoSeguimiento}")]
        public async Task<IActionResult> AperturaValija([FromBody] IEnumerable<ExpedienteGenerico> entrada, [FromRoute] string codigoSeguimiento)
        {

            try
            {
                List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
                List<string> ticketsAvanzar = new List<string>();


                foreach (var item in entrada)
                {
                    var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).Include(d => d.Documentos).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito && x.TipoExpediente == TipoExpediente.Complementario);
                    _wfService.AsignarVariable("EXPEDIENTE_FALTANTE", item.Faltante ? 1.ToString() : 0.ToString(), elExpediente.Credito.NumeroTicket);

                    if (item.Faltante)
                    {
                        string jsonEnv = ""; int controlador = 0; string enviocorreo = "";

                        foreach (var dc in elExpediente.Documentos)
                        {
                            if (!item.DocumentosPistoleados.Any(d => d == dc.Codificacion))
                            {
                                jsonEnv = jsonEnv + (controlador > 0 ? "," : "") + "\"" + dc.Codificacion + "\"";
                                enviocorreo += (controlador > 0 ? ", " : "") + dc.TipoDocumento.ToString();
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
                        var enviarA = _wfService.QuienCerroEtapa(ProcesoDocumentos.ETAPA_NOMINA_SET_COMPLEMENTARIO, ProcesoDocumentos.ETAPA_DESPACHO_OFICINA_DE_PARTES_SET_COMPLEMENTARIO, elExpediente.Credito.NumeroTicket);
                        await _mailService.SendEmail(enviarA.NormalizedEmail, "Notificación de Expediente con Faltantes: " + item.FolioCredito, mensaje);
                    }
                    var elExpedienteApertura = _context.ExpedientesComplementarios.Where(d => d.CreditoId == elExpediente.CreditoId).FirstOrDefault();

                    ticketsAvanzar.Add(elExpedienteApertura.NumeroTicket);
                }

                var laValija = _context.ValijasValoradas.FirstOrDefault(v => v.CodigoSeguimiento == codigoSeguimiento);
                laValija.MarcaAvance = "FN";
                _context.ValijasValoradas.Update(laValija);
                await _context.SaveChangesAsync();

                await _wfService.AvanzarSetComplementario(ProcesoDocumentos.ETAPA_NOMINA_SET_COMPLEMENTARIO, ProcesoDocumentos.ETAPA_APERTURA_VALIJA_SET_COMPLEMENTARIO, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));


                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

       

        [HttpPost("despacho-sucursal-oficiana-partes-sc")]
        public async Task<IActionResult> DespachoSucOfPartesSc([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {

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
                    CodigoSeguimiento = codSeg,
                    MarcaAvance = "OF_PARTES"
                };
                _context.ValijasValoradas.Add(valijaEnvio);
                await _context.SaveChangesAsync();

                foreach (var item in entrada)
                {
                    var credito = _context.Creditos.FirstOrDefault(x => x.FolioCredito == item.FolioCredito);
                    var elExpediente = _context.ExpedientesCreditos.FirstOrDefault(ex => ex.CreditoId == credito.Id && ex.TipoExpediente == TipoExpediente.Complementario);
                    elExpediente.ValijaValorada = valijaEnvio;
                    expedientesModificados.Add(elExpediente);
                    _wfService.AsignarVariable("USUARIO_DESPACHA_A_OF_PARTES", User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""), elExpediente.Credito.NumeroTicket);

                    var elExpedienteComplementario = _context.ExpedientesComplementarios.Where(d => d.CreditoId == credito.Id).FirstOrDefault();
                    ticketsAvanzar.Add(elExpedienteComplementario.NumeroTicket);


                }

                _context.ExpedientesCreditos.UpdateRange(expedientesModificados);
                await _context.SaveChangesAsync();

                await _wfService.AvanzarRango(ProcesoDocumentos.ETAPA_NOMINA_SET_COMPLEMENTARIO, ProcesoDocumentos.ETAPA_DESPACHO_OFICINA_DE_PARTES_SET_COMPLEMENTARIO, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));


                return Ok(valijaEnvio);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }




        [HttpPost("despacho-Documentos-opartes-Sucursal-sc")]
        public async Task<IActionResult> DespachoDocumentosSucursalSc([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {

            try
            {
                List<ExpedienteComplementario> expedientesModificados = new List<ExpedienteComplementario>();
                List<string> ticketsAvanzar = new List<string>();

                var codificacionOficinaLogedIn = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
                var oficinaEnvio = _context.Oficinas.FirstOrDefault(d => d.Codificacion == codificacionOficinaLogedIn);

                DateTime now = DateTime.Now;
                var codSeg = now.Ticks.ToString() + "X" + oficinaEnvio.Codificacion;

                var valijaEnvio = new ValijaValorada
                {
                    FechaEnvio = DateTime.Now,
                    Oficina = oficinaEnvio,
                    CodigoSeguimiento = codSeg,
                    MarcaAvance = "OF_PARTES_DOCUMENTOS"
                };
                _context.ValijasValoradas.Add(valijaEnvio);
                await _context.SaveChangesAsync();

                foreach (var item in entrada)
                {
                    var credito = _context.Creditos.FirstOrDefault(x => x.FolioCredito == item.FolioCredito);
                    //var elExpediente = _context.ExpedientesCreditos.FirstOrDefault(ex => ex.CreditoId == credito.Id && ex.TipoExpediente == TipoExpediente.Complementario);
                    //elExpediente.ValijaValorada = valijaEnvio;
                    //expedientesModificados.Add(elExpediente);
                   
                    var elExpedienteComplementario = _context.ExpedientesComplementarios.Where(d => d.CreditoId == credito.Id).FirstOrDefault();
                    elExpedienteComplementario.CodigoSeguimiento= codSeg;
                    ticketsAvanzar.Add(elExpedienteComplementario.NumeroTicket);
                    _wfService.AsignarVariable("USUARIO_DESPACHA_A_OF_PARTES_DOCUMENTOS_SET_COMPLEMENTARIO", User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""), elExpedienteComplementario.NumeroTicket);
                     expedientesModificados.Add(elExpedienteComplementario);

                }

                _context.ExpedientesComplementarios.UpdateRange(expedientesModificados);
                await _context.SaveChangesAsync();

                await _wfService.AvanzarSetComplementario(ProcesoDocumentos.ETAPA_NOMINA_SET_COMPLEMENTARIO, ProcesoDocumentos.ETAPA_DESPACHO_A_DOCUMENTOS_A_SUCURSAL, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));


                return Ok(valijaEnvio);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [HttpGet("obtener-expediente-sc/{folioCredito}/{etapaSolicitud?}")]
        public IActionResult ObtenerXpedienteSetComplementario([FromRoute] string folioCredito, [FromRoute] string etapaSolicitud = "")
        {
            try
            {
                /* TODO Validar que sea de la oficina y que este en la etapa correcta de solicitud*/
                var expediente = _context.ExpedientesCreditos.Where(p => p.TipoExpediente == TipoExpediente.Complementario).Include(x => x.Credito).Include(x => x.Documentos).FirstOrDefault(x => x.Credito.FolioCredito == folioCredito.Trim());
                var oficinaUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
                var ofipago = _wfService.ObtenerVariable("OFICINA_PAGO", expediente.Credito.NumeroTicket);
                var laOficinaPago = _context.Oficinas.Include(k => k.OficinaProceso).FirstOrDefault(ofi => ofi.Codificacion == ofipago);

                if (expediente.Documentos.Count() > 0 && ((oficinaUsuario == "A000") || (ofipago == oficinaUsuario || laOficinaPago.OficinaProceso.Codificacion == oficinaUsuario)))
                {
                    if (!string.IsNullOrEmpty(etapaSolicitud))
                    {
                        var etapaActual = _wfService.OntenerTareaActualSc("PROCESS", expediente.Credito.FolioCredito.Trim());
                        if (etapaActual.NombreInterno == etapaSolicitud)
                        {
                            return Ok(expediente);
                        }
                        else
                        {
                            return NotFound("Este Expediente esta en la siguiente tarea: <strong>" + etapaActual.Nombre + "</strong>");
                        }
                    }

                    return Ok(expediente);
                }
                else
                {
                    return NotFound("Carga del Expediente en la siguiente Oficina: " + laOficinaPago.Nombre);
                }

            }
            catch (Exception)
            {
                return NotFound("El Expediente aun no esta Cargado en Galvarino.");
            }
        }


        [HttpPost("preparar-nomina-sc")]
        public async Task<IActionResult> PrepararNominaSetComercial([FromBody] IEnumerable<ExpedienteGenerico> entrada)
        {

            List<string> ticketsAvanzar = new List<string>();
            foreach (var item in entrada)
            {
                 var ExpedienteCred = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito && x.TipoExpediente==TipoExpediente.Complementario);
                var elExpediente = _context.ExpedientesComplementarios.Where(d => d.CreditoId == ExpedienteCred.CreditoId).FirstOrDefault();
                ticketsAvanzar.Add(elExpediente.NumeroTicket);
            }

            await _wfService.AvanzarSetComplementario(ProcesoDocumentos.ETAPA_NOMINA_SET_COMPLEMENTARIO, ProcesoDocumentos.ETAPA_PREPARAR_NOMINA_SET_COMPLEMENTARIO, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));
            return Ok();
        }


        [HttpGet("listar-valijas-enviadas-sc/{marcavance?}")]
        public IActionResult ListarValijasSc(string marcavance = "")
        {
            var valijas = _solicitudRepository.listarValijasEnviadasSc(marcavance);

            return Ok(valijas);
        }
        [HttpGet("listar-valijas-documentos-oficina-ingreso-sc/{marcavance?}/{etapaIn?}")]
        public IActionResult ListarValijasOficinaIngresoSc(string marcavance = "",[FromRoute] string etapaIn = "")
        {
            var rolesUsuario = User.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToArray();
            var oficinaUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
            var ObjetoOficinaUsuario = _context.Oficinas.Include(of => of.OficinaProceso).FirstOrDefault(ofc => ofc.Codificacion == oficinaUsuario);
            var OficinasUsuario = _context.Oficinas.Where(ofc => ofc.OficinaProceso.Id == ObjetoOficinaUsuario.Id && ofc.EsMovil == true);
            var ofinales = new List<Oficina>();
            ofinales.Add(ObjetoOficinaUsuario);
            ofinales.AddRange(OficinasUsuario.ToList());
            var lasEtps = new string[] { etapaIn };
            var lasOff = ofinales.Select(x => x.Codificacion).ToArray();
           
            var valijas = _solicitudRepository.listarValijasEnviadasOficinaIngresoSc(marcavance, rolesUsuario, User.Identity.Name, lasOff, lasEtps);

            return Ok(valijas);
        }



        [HttpGet("nomina-setcomplementario/{etapaIn?}/{fechaCerrar?}")]
        public IActionResult ListarMisSolicitudes([FromRoute] string etapaIn = "", [FromRoute] string fechaCerrar = "", [FromQuery] int offset = 0, [FromQuery] int limit = 20, [FromQuery] string sort = "", [FromQuery] string order = "", [FromQuery] string sucursal = "")
        {

            //var rolUsuario =  //FirstOrDefault(x => x.Type == ClaimTypes.Role).Value;    

            var rolesUsuario = User.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToArray();
            var oficinaUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
            var ObjetoOficinaUsuario = _context.Oficinas.Include(of => of.OficinaProceso).FirstOrDefault(ofc => ofc.Codificacion == oficinaUsuario);
            var OficinasUsuario = _context.Oficinas.Where(ofc => ofc.OficinaProceso.Id == ObjetoOficinaUsuario.Id && ofc.EsMovil == true);
            var ofinales = new List<Oficina>();
            ofinales.Add(ObjetoOficinaUsuario);
            ofinales.AddRange(OficinasUsuario.ToList());

            var lasOff = ofinales.Select(x => x.Codificacion).ToArray();
            var lasEtps = new string[] { etapaIn };
            string orden = sort + " " + order;
            var salida = _solicitudRepository.listarSolicitudesSc(rolesUsuario, User.Identity.Name, lasOff, lasEtps, orden, fechaCerrar, sucursal);
            var lida = new
            {
                total = salida.Count(),
                rows = salida.Count() < offset ? salida : salida.Skip(offset).Take(limit).ToList()
            };

            return Ok(lida);
        }


        [HttpPost("solucion-reparo-sucursal-set-complementario")]
        public async Task<IActionResult> SolucionReparosSucursalSc([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<string> ticketsAvanzar = new List<string>();
            foreach (var item in entrada)
            {
                var elExpedienteCred = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito && x.TipoExpediente == TipoExpediente.Legal);

                var elExpediente = _context.ExpedientesComplementarios.Where(a => a.CreditoId == elExpedienteCred.CreditoId).SingleOrDefault();
                ticketsAvanzar.Add(elExpediente.NumeroTicket);
            }

            _wfService.ForzarAvance(ProcesoDocumentos.ETAPA_NOMINA_SET_COMPLEMENTARIO, ProcesoDocumentos.ETAPA_DESPACHO_OFICINA_DE_PARTES_SET_COMPLEMENTARIO, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));
            return Ok();
        }



        [Route("despacho-a-custodia-sc/caja-valorada/{codigoCaja}/agregar-documento/{folioDocumento}")]
        public async Task<IActionResult> AgregarDocumentoCajaValoradaSc([FromRoute] string codigoCaja, [FromRoute] string folioDocumento)
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

                var credi = _context.Creditos.FirstOrDefault(cre => cre.FolioCredito == folioCredito);
                var numeroTicket = _context.ExpedientesComplementarios.FirstOrDefault(cre => cre.CreditoId == credi.Id).NumeroTicket;
                var etapaDocumento = _wfService.OntenerTareaActual(ProcesoDocumentos.ETAPA_NOMINA_SET_COMPLEMENTARIO, numeroTicket);
                var asignadus = _context.Users.FirstOrDefault(us => us.Identificador == etapaDocumento.AsignadoA);

                if (etapaDocumento.Etapa.NombreInterno != ProcesoDocumentos.ETAPA_DESPACHO_A_CUSTODIA_SET_COMPLEMENTARIO)
                {
                    throw new Exception($"Etapa de documento no corresponde. Etapa Actual es: ${etapaDocumento.Etapa.Nombre} y esta asignada a: ${asignadus.Nombres}");
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

                /*TODO: Convertir en consulta dapper para manejar mejor performance */
                //var documentos = from pasoval in
                //                     from mono in _context.PasosValijasValoradas
                //                     join credit in _context.Creditos on mono.FolioCredito equals credit.FolioCredito
                //                     select new
                //                     {
                //                         mono.FolioCredito,
                //                         mono.CodigoCajaValorada,
                //                         mono.Usuario,
                //                         TotalDocumentos = credit.TipoCredito == TipoCredito.Normal ? 2 : 1
                //                     }
                //                 where pasoval.CodigoCajaValorada == codigoCaja && pasoval.Usuario == User.Identity.Name

                //                 group pasoval by new { pasoval.FolioCredito, pasoval.TotalDocumentos } into slt
                //                 select new
                //                 {
                //                     Folio = slt.Key.FolioCredito,
                //                     Pistoleados = slt.Count(),
                //                     Total = slt.Key.TotalDocumentos
                //                 };



                var documentos = _solicitudRepository.listarSegurosPorValijas(codigoCaja, User.Identity.Name);
                //var documentos = from pasoval in
                //                     from mono in _context.PasosValijasValoradas
                //                     join credit in _context.Creditos on mono.FolioCredito equals credit.FolioCredito
                //                     join expcred in _context.ExpedientesCreditos on credit.Id equals expcred.CreditoId
                //                     join seguros in _context.Documentos on expcred.Id equals seguros.ExpedienteCredito.Id
                //                     select new
                //                     {
                //                         mono.FolioCredito,
                //                         mono.CodigoCajaValorada,
                //                         mono.Usuario,
                //                         TotalDocumentos = credit.TipoCredito == TipoCredito.Normal ? 2 : 1
                //                     }
                //                 where pasoval.CodigoCajaValorada == codigoCaja && pasoval.Usuario == User.Identity.Name

                //                 group pasoval by new { pasoval.FolioCredito, pasoval.TotalDocumentos } into slt
                //                 select new
                //                 {
                //                     Folio = slt.Key.FolioCredito,
                //                     Pistoleados = slt.Count(),
                //                     Total = slt.Key.TotalDocumentos
                //                 };

                var salida =  documentos.OrderBy(d => d.Pistoleados).ToList();
                return Ok(salida);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [HttpPost("recepcion-custodia-sc/{codigoSeguimiento}")]
        public async Task<IActionResult> RecepcionCustodiaSc([FromBody] IEnumerable<ExpedienteGenerico> entrada, [FromRoute] string codigoSeguimiento)
        {
            List<string> ticketsAvanzar = new List<string>();
            foreach (var item in entrada)
            {
                var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito && x.TipoExpediente == TipoExpediente.Complementario);
                _wfService.AsignarVariable("EXPEDIENTE_FALTANTE_CUSTODIA", item.Faltante ? 1.ToString() : 0.ToString(), elExpediente.Credito.NumeroTicket);
                var expedienteComplementario = _context.ExpedientesComplementarios.Where(x => x.CreditoId == elExpediente.CreditoId).FirstOrDefault();
                ticketsAvanzar.Add(expedienteComplementario.NumeroTicket);

            }

            var laCaja = _context.CajasValoradas.FirstOrDefault(v => v.CodigoSeguimiento == codigoSeguimiento);
            laCaja.MarcaAvance = "FN";
            _context.CajasValoradas.Update(laCaja);
            await _context.SaveChangesAsync();
            await _wfService.AvanzarSetComplementario(ProcesoDocumentos.ETAPA_NOMINA_SET_COMPLEMENTARIO, ProcesoDocumentos.ETAPA_RECEPCION_Y_CUSTODIA_SET_COMPLEMENTARIO, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));
            return Ok();
        }





    }
}