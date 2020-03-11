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

namespace Galvarino.Web.Controllers
{
    [Route("wf/v1")]
    [Authorize]
    public class WorkflowController : Controller
    {
        private const int CantidadCaracteres = 23;
        private readonly ApplicationDbContext _context;
        private readonly IWorkflowService _wfService;
        
        public WorkflowController(ApplicationDbContext context, IWorkflowService wfservice)
        {
            _context = context;
            _wfService = wfservice;
        }

        [Route("mis-solicitudes/{etapaIn?}")]
        public IActionResult Index([FromRoute] string etapaIn = "")
        {
            ViewData["etapaIn"] = etapaIn;
            return View();
        }

                

        #region Etapas

        [Route("despacho-oficina-matriz")]
        public IActionResult DespachoOfNotaria()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }

        [Route("recepcion-expedientes/{folioCaja?}")]
        public IActionResult RecepcionSetLegal(string folioCaja = "")
        {

            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            ViewBag.folioCaja = folioCaja;
            return View();
        }

        [Route("envio-a-notaria")]
        public IActionResult EnvioNotaria()
        {
            var comunaOficina = _context.Users.Include(us => us.Oficina).ThenInclude(of => of.Comuna).FirstOrDefault(usr => usr.UserName == User.Identity.Name).Oficina.Comuna;
            var notariasComuna = _context.Notarias.Where(not => not.Comuna.Id == comunaOficina.Id).ToList();
            ViewBag.NotariasOficina = notariasComuna;
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }

        [Route("recepciona-notaria")]
        public IActionResult RecepcionaNotaria()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }

        [Route("revision-documentos")]
        public IActionResult RevisionDocumentos()
        {
            var comunaOficina = _context.Users.Include(us => us.Oficina).ThenInclude(of => of.Comuna).FirstOrDefault(usr => usr.UserName == User.Identity.Name).Oficina.Comuna;
            var notariasComuna = _context.Notarias.Where(not => not.Comuna.Id == comunaOficina.Id).ToList();
            ViewBag.NotariasOficina = notariasComuna;
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }

        [Route("despacho-reparo-notaria")]
        public IActionResult DespachoReparoNotaria()
        {
            var comunaOficina = _context.Users.Include(us => us.Oficina).ThenInclude(of => of.Comuna).FirstOrDefault(usr => usr.UserName == User.Identity.Name).Oficina.Comuna;
            var notariasComuna = _context.Notarias.Where(not => not.Comuna.Id == comunaOficina.Id).ToList();
            ViewBag.NotariasOficina = notariasComuna;
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }


        [Route("despacho-sucursal-oficiana-partes")]
        public IActionResult DespachoSucOfPartes()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }

        [Route("recepcion-oficina-partes")]
        public IActionResult RecepcionOfPartes()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }

        [Route("recepcion-valija")]
        public IActionResult RecepcionValija()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }

        [Route("apertura-valija")]
        public IActionResult AperturaValija()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }

        [Route("analisis-mesa-control")]
        public IActionResult AnalisisMesaControl()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }


        [Route("analisis-mesa-control-nomina-especial")]
        public IActionResult AnalisisMesaControlNominaEspecial()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }



        [Route("solucion-expediente-faltante")]
        public IActionResult SolucionExpedienteFaltante()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }

        [Route("despacho-reparo-oficina-partes")]
        public IActionResult DespachoOfPtReparoSucursal()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            var mistareas = _context.Tareas.Include(d => d.Etapa).Include(f => f.Solicitud).Where(x => (x.AsignadoA == User.Identity.Name.ToUpper().Replace(@"LAARAUCANA\", "") || x.Etapa.TipoUsuarioAsignado == TipoUsuarioAsignado.Rol && User.IsInRole(x.AsignadoA)) && x.Estado == EstadoTarea.Activada && x.Etapa.NombreInterno == "DESPACHO_OF_PARTES_DEVOLUCION").ToList();

            var salida = new List<Oficina>();
           
            foreach (var tarea in mistareas)
            {
                var codigoOficinaDevolucion = _wfService.ObtenerVariable("OFINA_PROCESA_NOTARIA", tarea.Solicitud.NumeroTicket);
                var oficinaDevolucion = _context.Oficinas.FirstOrDefault(o => o.Codificacion == codigoOficinaDevolucion);
                //salida.Add(oficinaDevolucion);

                if (!salida.Any(c => c.Codificacion == codigoOficinaDevolucion))
                {
                    salida.Add(oficinaDevolucion);
                }
            }

            ViewBag.OficinasDevolucion = salida;
            return View();
        }

        [Route("despacho-oficina-correccion")]
        public IActionResult DespachoOfCorreccion()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }

        [Route("solucion-reparo-sucursal")]
        public IActionResult SolucionReparosSucursal()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }

        [Route("despacho-a-custodia")]
        public IActionResult DespachoCustodia()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }


        [Route("despacho-a-custodia-nomina-especial")]
        public IActionResult DespachoCustodiaNominaEspecial()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }

        [Route("recepcion-custodia/{folioCaja?}")]
        public IActionResult RecepcionCustodia(string folioCaja = "")
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            ViewBag.folioCaja = folioCaja;
            
            return View();
        }


        [Route("preparar-nomina")]
        public IActionResult PrepararNomina()
        {
            var comunaOficina = _context.Users.Include(us => us.Oficina).ThenInclude(of => of.Comuna).FirstOrDefault(usr => usr.UserName == User.Identity.Name).Oficina.Comuna;
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            
            return View();
        }


       

        [Route("envio-a-notaria-rm")]
        public IActionResult EnvioNotariaRM()
        {
            var comunaOficina = _context.Users.Include(us => us.Oficina).ThenInclude(of => of.Comuna).FirstOrDefault(usr => usr.UserName == User.Identity.Name).Oficina.Comuna;
            var notariasComuna = _context.Notarias.Where(not => not.Comuna.Id == comunaOficina.Id).ToList();
            ViewBag.NotariasOficina = notariasComuna;
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }

        [Route("recepciona-notaria-rm")]
        public IActionResult RecepcionaNotariaRM()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }

        [Route("revision-documentos-rm")]
        public IActionResult RevisionDocumentosRM()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }

        [Route("devolucion-reparo-notaria-rm")]
        public IActionResult DevolucionReparoNotariaRM()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }

        [Route("almacenaje-set-comercial")]
        public IActionResult AlmacenajeSetComercial()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }

        [Route("almacenaje-set-comercial/historial")]
        public IActionResult HistorialAlmacenajeSetComercial()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }


        [Route("marcar-documentos-legalizados")]
        public IActionResult DocumentosLegalizados()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }

        [Route("notas-internas/generadas/{tipoDocumento}")]
        public IActionResult DocumentosGenerados(string tipoDocumento)
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            ViewBag.tipoDocumento = tipoDocumento;
            return View();
        }

        [Route("reasignaciones/oficinas")]
        public IActionResult ReasignaOficina()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            ViewBag.Offices =  _context.Oficinas.ToList();
            return View();
        }

        [Route("reasignar-etapa-credito")]
        public IActionResult ModifcarEstado()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            ViewBag.Offices = _context.Oficinas.ToList();
            return View();
        }



        #endregion
    }
}
