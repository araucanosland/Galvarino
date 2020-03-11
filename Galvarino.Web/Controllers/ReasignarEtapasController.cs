using Galvarino.Web.Data;
using Galvarino.Web.Services.Workflow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galvarino.Web.Controllers
{

    [Route("configuracion")]
    [Authorize]
    public class ReasignarEtapasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWorkflowService _wfService;
        public ReasignarEtapasController(ApplicationDbContext context, IWorkflowService wfservice)
        {
            _context = context;
            _wfService = wfservice;
        }


        [Route("reasignar-etapa-credito")]
        public IActionResult ReasignarEtapa(string folioCredito = "")
        {

            return View();
        }



    }

}