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
    [Route("api/app/v1")]
    [ApiController]
    public class BusinessController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        public BusinessController(ApplicationDbContext context)
        {
            _context = context;
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


        [HttpGet("listar-valijas-enviadas/{marcavance?}")]
        public IActionResult ListarValijas(string marcavance="")
        {
            var valijas = _context.ValijasValoradas.Include(v => v.Expedientes).Include(v => v.Oficina).ToList();

            if(!string.IsNullOrEmpty(marcavance)){
                valijas = valijas.Where(d => d.MarcaAvance == marcavance).ToList();
            }

            return Ok(valijas);
        }

       
        
    }
}
