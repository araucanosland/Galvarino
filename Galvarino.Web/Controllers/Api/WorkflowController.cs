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

        [HttpGet("mis-solicitudes")]
        public async Task<IActionResult> ListarMisSolicitudes(){

            var mistareas = _context.Tareas.Include(d => d.Etapa).Include(f => f.Solicitud).Where(x => x.AsignadoA == "17042783-1" && x.Estado == EstadoTarea.Activada);
            var salida = new List<dynamic>();
            
            await mistareas.ForEachAsync(tarea => {

                var folioCredito = _context.Variables.FirstOrDefault(c => c.Clave == "FOLIO_CREDITO" && c.NumeroTicket == tarea.Solicitud.NumeroTicket).Valor;
                var credito = _context.Creditos.FirstOrDefault(cre => cre.FolioCredito == folioCredito);

                salida.Add(new {
                    tarea = tarea,
                    credito = credito
                });
            });

            
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

        [Route("recepcion-set-legal")]
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

        [Route("envio-a-notaria")]
        public async Task<IActionResult> EnvioNotaria([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {

            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();
            var notariaEnvio = _context.Notarias.Find(1);
            var oficinaEnvio = _context.Oficinas.Find(3);

            var packNotaria = new PackNotaria
            {
                FechaEnvio = DateTime.Now,
                NotariaEnvio = notariaEnvio,
                Oficina = oficinaEnvio
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

            return Ok();
        }

        [Route("recepciona-notaria")]
        public async Task<IActionResult> RecepcionaNotaria([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();
            //var oficinaEnvio = _context.Oficinas.Find(3);

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

        [Route("revision-documentos")]
        public async Task<IActionResult> RevisionDocumentos([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();
            //var oficinaEnvio = _context.Oficinas.Find(3);

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

        [Route("despacho-reparo-notaria")]
        public async Task<IActionResult> DespachoReparoNotaria([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();
            //var oficinaEnvio = _context.Oficinas.Find(3);

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

        [Route("despacho-sucursal-oficiana-partes")]
        public async Task<IActionResult> DespachoSucOfPartes([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();
            //var oficinaEnvio = _context.Oficinas.Find(3);

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

        [Route("recepcion-oficina-partes")]
        public async Task<IActionResult> RecepcionOfPartes([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();
            //var oficinaEnvio = _context.Oficinas.Find(3);

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

        [Route("recepcion-valija")]
        public async Task<IActionResult> RecepcionValija([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();
            //var oficinaEnvio = _context.Oficinas.Find(3);

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

        [Route("analisis-mesa-control")]
        public async Task<IActionResult> AnalisisMesaControl([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();
            //var oficinaEnvio = _context.Oficinas.Find(3);

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

        [Route("solucion-expediente-faltante")]
        public async Task<IActionResult> SolucionExpedienteFaltante([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();
            //var oficinaEnvio = _context.Oficinas.Find(3);

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

        [Route("despacho-reparo-oficina-partes")]
        public async Task<IActionResult> DespachoOfPtReparoSucursal([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();
            //var oficinaEnvio = _context.Oficinas.Find(3);

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

        [Route("despacho-oficina-correccion")]
        public async Task<IActionResult> DespachoOfCorreccion([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();
            //var oficinaEnvio = _context.Oficinas.Find(3);

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

        [Route("solucion-reparo-sucursal")]
        public async Task<IActionResult> SolucionReparosSucursal([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();
            //var oficinaEnvio = _context.Oficinas.Find(3);

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

        [Route("despacho-a-custodia")]
        public async Task<IActionResult> DespachoCustodia([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();
            //var oficinaEnvio = _context.Oficinas.Find(3);

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

        [Route("recepcion-custodia")]
        public async Task<IActionResult> RecepcionCustodia([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
        {
            List<ExpedienteCredito> expedientesModificados = new List<ExpedienteCredito>();
            List<string> ticketsAvanzar = new List<string>();
            //var oficinaEnvio = _context.Oficinas.Find(3);

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
