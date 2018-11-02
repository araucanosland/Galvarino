using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Galvarino.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Galvarino.Web.Models.Workflow;
using Galvarino.Web.Models.Helper;
using Galvarino.Web.Models.Application;

namespace Galvarino.Web.Controllers
{
    [Route("configuraciones/workflow")]
    [Authorize]
    public class WorkflowAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        public WorkflowAdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("procesos")]
        public IActionResult Procesos()
        {
            ViewBag.procesos = _context.Procesos.ToList();
            return View();
        }

        [Route("procesos/{procesoId}/etapas")]
        public IActionResult EtapasFlujo([FromRoute] int procesoId)
        {
            ViewBag.proceso = _context.Procesos.Find(procesoId);
            ViewBag.proceso.Etapas = _context.Etapas
                                                .Include(e => e.Proceso)
                                                .Include(e => e.Actuales)
                                                .Include(x => x.Destinos)
                                                .Where(e => e.Proceso.Id == procesoId).ToList();
            return View();
        }

        [Route("procesos/{procesoId}/etapas/formulario/{etapaId?}")]
        public IActionResult Etapa([FromRoute] int procesoId, [FromRoute] int etapaId = 0)
        {
            ViewBag.Id = etapaId;
            ViewBag.ProcesoId = procesoId;
            ViewBag.proceso = _context.Procesos.FirstOrDefault(prc => prc.Id == procesoId);
            ViewBag.etapa = _context.Etapas.Include(d => d.Proceso).Include(e => e.Destinos).FirstOrDefault(e => e.Id == etapaId && e.Proceso.Id == procesoId);
            ViewBag.editando = etapaId > 0;
            ViewBag.tiposEtapa = Enum.GetValues(typeof(TipoEtapa)).Cast<TipoEtapa>().ToList();
            ViewBag.tiposAsignado = Enum.GetValues(typeof(TipoUsuarioAsignado)).Cast<TipoUsuarioAsignado>().ToList();
            ViewBag.tiposDuracion = Enum.GetValues(typeof(TipoDuracion)).Cast<TipoDuracion>().ToList();
            ViewBag.listadoEtapas = _context.Etapas.Include(d => d.Proceso).Where(e => e.Proceso.Id == procesoId);

            return View();
        }

        
    }
}