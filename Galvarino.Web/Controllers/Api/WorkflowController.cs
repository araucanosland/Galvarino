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
        public IActionResult ListarMisSolicitudes(){
            var mistareas = _context.Tareas.Where(x => x.AsignadoA=="17042783-1" && x.Estado == EstadoTarea.Activada);
            return Ok(mistareas);
        }


        [HttpPost("despacho-oficina-a-notaria")]
        public async Task<IActionResult> DespachoOfNotaria([FromBody] IEnumerable<EnvioNotariaFormHelper> entrada)
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

        [Route("recepcion-set-legal")]
        public IActionResult RecepcionSetLegal()
        {
            return Ok();
        }

        [Route("envio-a-notaria")]
        public IActionResult EnvioNotaria()
        {
            return Ok();
        }

        [Route("recepciona-notaria")]
        public IActionResult RecepcionaNotaria()
        {
            return Ok();
        }

        [Route("revision-documentos")]
        public IActionResult RevisionDocumentos()
        {
            return Ok();
        }

        [Route("despacho-reparo-notaria")]
        public IActionResult DespachoReparoNotaria()
        {
            return Ok();
        }

        [Route("despacho-sucursal-oficiana-partes")]
        public IActionResult DespachoSucOfPartes()
        {
            return Ok();
        }

        [Route("recepcion-oficina-partes")]
        public IActionResult RecepcionOfPartes()
        {
            return Ok();
        }

        [Route("recepcion-valija")]
        public IActionResult RecepcionValija()
        {
            return Ok();
        }

        [Route("analisis-mesa-control")]
        public IActionResult AnalisisMesaControl()
        {
            return Ok();
        }

        [Route("solucion-expediente-faltante")]
        public IActionResult SolucionExpedienteFaltante()
        {
            return Ok();
        }

        [Route("despacho-reparo-oficina-partes")]
        public IActionResult DespachoOfPtReparoSucursal()
        {
            return Ok();
        }

        [Route("despacho-oficina-correccion")]
        public IActionResult DespachoOfCorreccion()
        {
            return Ok();
        }

        [Route("solucion-reparo-sucursal")]
        public IActionResult SolucionReparosSucursal()
        {
            return Ok();
        }

        [Route("despacho-a-custodia")]
        public IActionResult DespachoCustodia()
        {
            return Ok();
        }

        [Route("recepcion-custodia")]
        public IActionResult RecepcionCustodia()
        {
            return Ok();
        }
    }
}
