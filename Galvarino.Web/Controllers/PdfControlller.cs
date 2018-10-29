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
using Galvarino.Web.Models.Helper;

namespace Galvarino.Web.Controllers
{
    [Route("salidas/pdf")]
    [Authorize]
    public class PdfController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWorkflowService _wfService;

        public PdfController(ApplicationDbContext context, IWorkflowService wfservice)
        {
            _context = context;
            _wfService = wfservice;
        }

        [Route("test")]
        public IActionResult Index()
        {
            
            return new ViewAsPdf(new PdfModelHelper(){
                FechaImpresion = DateTime.Now.ToShortDateString()
            })
            {
                PageSize = Size.Letter
            };
        }


        [Route("detalle-pack-notaria/{codigoSeguimiento}")]
        public async Task<IActionResult> DetallePackNotaria(string codigoSeguimiento)
        {
            var pack = await _context.PacksNotarias
                                .Include(pm => pm.Expedientes)  
                                    .ThenInclude(e => e.Credito)
                                .Include(pm => pm.Expedientes)
                                    .ThenInclude(e => e.Documentos)    
                                .Include(pm => pm.NotariaEnvio)
                                .Include(pm => pm.Oficina)
                                .FirstOrDefaultAsync(pn => pn.CodigoSeguimiento == codigoSeguimiento);


            return new ViewAsPdf(new PdfModelHelper()
            {
                FechaImpresion = DateTime.Now.ToShortDateString(),
                CodigoSeguimiento = codigoSeguimiento,
                PackNotaria = pack
            })
            {
                PageSize = Size.Letter
            };
        }

        public IActionResult DetalleValijaValorada()
        {
            return new ViewAsPdf
            {
                PageSize = Size.Letter
            };
        }

        public IActionResult DetalleCajaValorada()
        {
            return new ViewAsPdf
            {
                PageSize = Size.Letter
            };
        }

        public IActionResult DetalleValijaValoradaOficina()
        {
            return new ViewAsPdf
            {
                PageSize = Size.Letter
            };
        }
    }
}
