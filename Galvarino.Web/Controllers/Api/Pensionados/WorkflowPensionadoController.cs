using DocumentFormat.OpenXml.Math;
using Galvarino.Web.Data;
using Galvarino.Web.Data.Repository;
using Galvarino.Web.Models.Application;
using Galvarino.Web.Models.Application.Pensionado;
using Galvarino.Web.Models.Helper;
using Galvarino.Web.Models.Helper.Pensionado;
using Galvarino.Web.Models.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite.Internal.ApacheModRewrite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpedienteGenerico = Galvarino.Web.Models.Helper.Pensionado.ExpedienteGenerico;

namespace Galvarino.Web.Controllers.Api.Pensionados
{
    [Route("api/pensionado/wf/v1")]
    [ApiController]
    public class WorkflowPensionadoController : ControllerBase
    {

        private readonly ISolicitudPensionadoRepository _solicitudPensionadoRepository;
        private readonly PensionadoDbContext _context;
        
        public WorkflowPensionadoController(ISolicitudPensionadoRepository solicitudPensionadoRepository, PensionadoDbContext context)
        { 
            _solicitudPensionadoRepository = solicitudPensionadoRepository;
            _context = context;
        }



        [HttpGet("mis-solicitudes/{etapaIn?}")]
        public IActionResult ListarMisSolicitudes([FromRoute] string etapaIn = "", [FromQuery] int offset = 0, [FromQuery] int limit = 20)
        {
            var salida = _solicitudPensionadoRepository.ListarSolicitudes(etapaIn);
            var lida = new
            {
                total = salida.Count(),
                rows = salida.Count() < offset ? salida : salida.Skip(offset).Take(limit).ToList()
            };
            return Ok(lida);

        }

        [HttpGet("obtener-expediente/{folio}/{etapaSolicitud?}")]
        public IActionResult ObtenerXpediente([FromRoute] string folio, [FromRoute] string etapaSolicitud = "")
        {
            try
            {
                ///* TODO Validar que sea de la oficina y que este en la etapa correcta de solicitud*/
                var oficinaUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
                var expediente = _solicitudPensionadoRepository.ObtenerExpedientes(folio, etapaSolicitud, oficinaUsuario.ToString()) ;

                return Ok(expediente);

            }
            catch (Exception)
            {
                return NotFound("El Expediente aun no esta Cargado en Galvarino.");
            }
        }


        [HttpPost("preparar-nomina")]
        public IActionResult PrepararNomina([FromBody] IEnumerable<ExpedienteGenerico> entrada)
        {

            List<string> ticketsAvanzar = new List<string>();
            foreach (var item in entrada)
            {
                var elExpediente = _context.Expedientes.Include(d => d.Pensionado).SingleOrDefault(x => x.Pensionado.Folio == item.Folio);
                ticketsAvanzar.Add(elExpediente.Pensionado.NumeroTicket);
            }
            
            var salida = _solicitudPensionadoRepository.AvanzarRango(ProcesoDocumentos.NOMBRE_PROCESO_PENSIONADO, ProcesoDocumentos.ETAPA_PREPARAR_NOMINA, ticketsAvanzar, User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", ""));
            return Ok();
        }

        [HttpPost("despacho-sucursal-oficiana-partes")]
        public IActionResult DespachoOfPartes([FromBody] IEnumerable<ColeccionDespachoPartes> entrada)
        {
            var user = User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", "");
            var nombreProceso = ProcesoDocumentos.NOMBRE_PROCESO_PENSIONADO;
            var etapa = ProcesoDocumentos.ETAPA_DESPACHO_OFICINA_PARTES;
            var identificacionUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
            ValijasValoradas salida = _solicitudPensionadoRepository.DespachoOfPartes(nombreProceso,etapa, entrada, identificacionUsuario, user);
            
            return Ok(salida);
        }


    }
}
