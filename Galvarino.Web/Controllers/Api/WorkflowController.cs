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


namespace Galvarino.Web.Controllers.Api
{
    [Route("api/wf/v1")]
    [ApiController]
    public class WorkflowController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly IWorkflowService _wfService;
        public WorkflowController(ApplicationDbContext context, IWorkflowService wfservice)
        {
            _context = context;
            _wfService = wfservice;
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

            var mistareas = _context.Tareas.Include(d => d.Etapa).Include(f => f.Solicitud).Where(x => x.AsignadoA == "17042783-1" && x.Estado == EstadoTarea.Activada);
            
            if(!string.IsNullOrEmpty(etapaIn)){
                mistareas = mistareas.Where(g => g.Etapa.NombreInterno==etapaIn);
            }
            
            var salida = new List<dynamic>();
            await mistareas.ForEachAsync(tarea => {

                var folioCredito = _wfService.ObtenerVariable("FOLIO_CREDITO", tarea.Solicitud.NumeroTicket);
                var motivoDevol = _wfService.ObtenerVariable("CODIGO_MOTIVO_DEVOLUCION_A_SUCURSAL", tarea.Solicitud.NumeroTicket);
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
                    reparo=motivoDevol.Length > 0 ? Convert.ToInt32(motivoDevol): 0
                });
            });
            
            
            return Ok(salida);
        }

        [HttpGet("solicitudes-reparo-devolicion/{oficina?}")]
        public async Task<IActionResult> ListarOficinasReparo(string oficina="")
        {

            var mistareas = _context.Tareas.Include(d => d.Etapa).Include(f => f.Solicitud).Where(x => x.AsignadoA == "17042783-1" && x.Estado == EstadoTarea.Activada && x.Etapa.NombreInterno == "DESPACHO_OF_PARTES_DEVOLUCION");

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



        [HttpPost("despacho-oficina-a-notaria")]
        public async Task<IActionResult> DespachoOfNotaria([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();
            var oficinaEnvio = _context.Oficinas.Find(3);

            foreach (var item in entrada)
            {
                var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                expedientesModificados.Add(elExpediente);
                ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
            }

            _context.ExpedientesCreditos.UpdateRange(expedientesModificados);
            await _context.SaveChangesAsync();
            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DESPACHO_OFICINA_NOTARIA, ticketsAvanzar, "17042783-1");
            
            return Ok();
        }

        [HttpPost("recepcion-set-legal")]
        public async Task<IActionResult> RecepcionSetLegal([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();
            var oficinaEnvio = _context.Oficinas.Find(3);

            foreach (var item in entrada)
            {
                var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                expedientesModificados.Add(elExpediente);
                ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
            }

            _context.ExpedientesCreditos.UpdateRange(expedientesModificados);
            await _context.SaveChangesAsync();
            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_RECEPCION_SET_LEGAL, ticketsAvanzar, "17042783-1");
            return Ok();
        }

        [HttpPost("envio-a-notaria")]
        public async Task<IActionResult> EnvioNotaria([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {

            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();
            var notariaEnvio = _context.Notarias.Find(1);
            var oficinaEnvio = _context.Oficinas.Find(3);

            
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

            foreach (var item in entrada)
            {
                var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                elExpediente.PackNotaria = packNotaria;
                expedientesModificados.Add(elExpediente);
                ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
            }

            _context.ExpedientesCreditos.UpdateRange(expedientesModificados);
            await _context.SaveChangesAsync();
            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_ENVIO_NOTARIA, ticketsAvanzar, "17042783-1");

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

            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_RECEPCION_NOTARIA, ticketsAvanzar, "17042783-1");
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

            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_REVISION_DOCUMENTOS, ticketsAvanzar, "17042783-1");
            return Ok(entrada);
        }

        [HttpPost("despacho-reparo-notaria")]
        public async Task<IActionResult> DespachoReparoNotaria([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();
            var notariaEnvio = _context.Notarias.Find(1);
            var oficinaEnvio = _context.Oficinas.Find(3);

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
            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DESPACHO_REPARO_NOTARIA, ticketsAvanzar, "17042783-1");
            return Ok(packNotaria);
        }

        [HttpPost("despacho-sucursal-oficiana-partes")]
        public async Task<IActionResult> DespachoSucOfPartes([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();
            var oficinaEnvio = _context.Oficinas.Find(3);
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
            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DESPACHO_OFICINA_PARTES, ticketsAvanzar, "17042783-1");
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

            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_RECEPCION_VALIJA_OFICINA_PARTES, ticketsAvanzar, "17042783-1");

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

            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_RECEPCION_VALIJA_MESA_CONTROL, ticketsAvanzar, "17042783-1");

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
                var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                _wfService.AsignarVariable("EXPEDIENTE_FALTANTE", item.Faltante ? "1": "0", elExpediente.Credito.NumeroTicket);
                ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
            }

            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_APERTURA_VALIJA, ticketsAvanzar, "17042783-1");


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

            foreach (var item in entrada)
            {
                var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                _wfService.AsignarVariable("DEVOLUCION_A_SUCURSAL", item.Reparo > 0 ? "1":"0", elExpediente.Credito.NumeroTicket);
                if(item.Reparo > 0){
                    _wfService.AsignarVariable("CODIGO_MOTIVO_DEVOLUCION_A_SUCURSAL", item.Reparo.ToString(), elExpediente.Credito.NumeroTicket);
                }
                ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
            }

            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_ANALISIS_MESA_CONTROL, ticketsAvanzar, "17042783-1");
            return Ok();
        }

        [Route("solucion-expediente-faltante")]
        public async Task<IActionResult> SolucionExpedienteFaltante([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<string> ticketsAvanzar = new List<string>();

            foreach (var item in entrada)
            {
                var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
            }

            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DOCUMENTACION_FALTANTE, ticketsAvanzar, "17042783-1");
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
            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DESPACHO_OF_PARTES_DEVOLUCION, ticketsAvanzar, "17042783-1");
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

            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DESPACHO_OFICINA_CORRECCION, ticketsAvanzar, "17042783-1");

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

            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_SOLUCION_REPAROS_SUCURSAL, ticketsAvanzar, "17042783-1");
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
            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_DESPACHO_A_CUSTODIA, ticketsAvanzar, "17042783-1");
            return Ok(cajaval);
        }

        [Route("recepcion-custodia")]
        public async Task<IActionResult> RecepcionCustodia([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();

            foreach (var item in entrada)
            {
                var elExpediente = _context.ExpedientesCreditos.Include(d => d.Credito).SingleOrDefault(x => x.Credito.FolioCredito == item.FolioCredito);
                expedientesModificados.Add(elExpediente);
                ticketsAvanzar.Add(elExpediente.Credito.NumeroTicket);
            }

            _context.ExpedientesCreditos.UpdateRange(expedientesModificados);
            await _context.SaveChangesAsync();
            await _wfService.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO, ProcesoDocumentos.ETAPA_RECEPCION_NOTARIA, ticketsAvanzar, "17042783-1");
            return Ok();
        }
    }
}
