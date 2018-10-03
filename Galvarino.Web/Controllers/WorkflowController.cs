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

        [Route("mis-solicitudes/{etapaIn?}")]
        public IActionResult Index([FromRoute] string etapaIn = "")
        {
            ViewData["etapaIn"] = etapaIn;
            return View();
        }

                

        #region Etapas

        [Route("despacho-oficina-a-notaria")]
        public IActionResult DespachoOfNotaria()
        {
            ViewBag.CantidadCaracteresFolio = 23;
            return View();
        }

        [Route("recepcion-set-legal")]
        public IActionResult RecepcionSetLegal()
        {
            return View();
        }

        [Route("envio-a-notaria")]
        public IActionResult EnvioNotaria()
        {
            return View();
        }

        [Route("recepciona-notaria")]
        public IActionResult RecepcionaNotaria()
        {
            return View();
        }

        [Route("revision-documentos")]
        public IActionResult RevisionDocumentos()
        {
            return View();
        }

        [Route("despacho-reparo-notaria")]
        public IActionResult DespachoReparoNotaria()
        {
            return View();
        }

        [Route("despacho-sucursal-oficiana-partes")]
        public IActionResult DespachoSucOfPartes()
        {
            return View();
        }

        [Route("recepcion-oficina-partes")]
        public IActionResult RecepcionOfPartes()
        {
            return View();
        }

        [Route("recepcion-valija")]
        public IActionResult RecepcionValija()
        {
            return View();
        }

        [Route("analisis-mesa-control")]
        public IActionResult AnalisisMesaControl()
        {
            return View();
        }

        [Route("solucion-expediente-faltante")]
        public IActionResult SolucionExpedienteFaltante()
        {
            return View();
        }

        [Route("despacho-reparo-oficina-partes")]
        public IActionResult DespachoOfPtReparoSucursal()
        {
            return View();
        }

        [Route("despacho-oficina-correccion")]
        public IActionResult DespachoOfCorreccion()
        {
            return View();
        }

        [Route("solucion-reparo-sucursal")]
        public IActionResult SolucionReparosSucursal()
        {
            return View();
        }

        [Route("despacho-a-custodia")]
        public IActionResult DespachoCustodia()
        {
            return View();
        }

        [Route("recepcion-custodia")]
        public IActionResult RecepcionCustodia()
        {
            return View();
        }
        #endregion
    }
}
