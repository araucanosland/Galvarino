using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Galvarino.Web.Models;
using Galvarino.Web.Data;
using Microsoft.EntityFrameworkCore;
using Galvarino.Web.Services.Workflow;
using Galvarino.Web.Models.Application;
using Galvarino.Web.Models.Workflow;
using Microsoft.AspNetCore.Authorization;
using Galvarino.Web.Services.Notification;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;

namespace Galvarino.Web.Controllers
{
    [Route("salidas/pdf")]
    [Authorize]
    public class PdfController : Controller
    {
        private const int CantidadCaracteres = 24;
        private readonly ApplicationDbContext _context;
        private readonly IWorkflowService _wfService;

        public PdfController(ApplicationDbContext context, IWorkflowService wfservice)
        {
            _context = context;
            _wfService = wfservice;
        }

        public IActionResult Index()
        {
            return new ViewAsPdf{
                PageSize = Size.Letter
            };
        }
    }
}
