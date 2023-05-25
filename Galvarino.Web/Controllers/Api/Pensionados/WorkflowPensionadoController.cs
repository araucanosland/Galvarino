using DocumentFormat.OpenXml.Math;
using Galvarino.Web.Data;
using Galvarino.Web.Data.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

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
        public IActionResult ListarMisSolicitudes([FromRoute] string etapaIn = "")
        {


            //etapaIn = "PREPARAR_NOMINA";
            var salida = _solicitudPensionadoRepository.ListarSolicitudes(etapaIn);
            int offset = 0;
            int limit = 20;
            var lida = new
            {
                total = salida.Count(),
                rows = salida.Skip(offset).Take(limit).ToList()
            };
            return Ok(lida);


            //return Ok(respuesta);
        }

    }
}
