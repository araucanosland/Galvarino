using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Galvarino.Web.Models;

namespace Galvarino.Web.Controllers
{
    [Route("wf/v1")]
    public class WorkflowController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult DespachoOfNotaria()
        {
            return View();
        }

        public IActionResult RecepcionSetLegal()
        {
            return View();
        }

        public IActionResult EnvioNotaria()
        {
            return View();
        }

        public IActionResult RecepcionaNotaria()
        {
            return View();
        }

        public IActionResult RevisionDocumentos()
        {
            return View();
        }

        public IActionResult DespachoReparoNotaria()
        {
            return View();
        }

        public IActionResult DespachoSucOfPartes()
        {
            return View();
        }

        public IActionResult RecepcionOfPartes()
        {
            return View();
        }

        public IActionResult RecepcionValija()
        {
            return View();
        }

        public IActionResult AnalisisMesaControl()
        {
            return View();
        }

        public IActionResult SolucionExpedienteFaltante()
        {
            return View();
        }

        public IActionResult DespachoOfPtReparoSucursal()
        {
            return View();
        }

        public IActionResult DespachoOfCorreccion()
        {
            return View();
        }

        public IActionResult SolucionReparosSucursal()
        {
            return View();
        }

        public IActionResult DespachoCustodia()
        {
            return View();
        }

        public IActionResult RecepcionCustodia()
        {
            return View();
        }

    }
}
