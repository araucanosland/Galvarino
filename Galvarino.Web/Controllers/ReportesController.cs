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
    [Route("reportes")]
    [Authorize]
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ReportesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("workflow/carga-inicial/{fechaBuscar?}")]
        public IActionResult CargaInicial(string fechaBuscar = "")
        {
            //DateTime fecb = string.IsNullOrEmpty(fechaBuscar) ? DateTime.Now : Convert.ToDateTime(fechaBuscar);
            
            ViewBag.CargasIniciales = _context.CargasIniciales.ToList();
            
                        /*.Where(x => 
                            x.FechaCarga.Year == fecb.Year
                        &&  x.FechaCarga.Month == fecb.Month
                        &&  x.FechaCarga.Day == fecb.Day).ToList();*/
            return View();
        }



        


    }
}