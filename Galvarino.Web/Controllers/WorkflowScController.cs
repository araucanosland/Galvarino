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
using Galvarino.Web.Models.Helper;
using Rotativa.AspNetCore.Options;
using Galvarino.Web.Data.Repository;

namespace Galvarino.Web.Controllers
{
    [Route("wf/v1")]
    [Authorize]
    public class WorkflowScController : Controller
    {
        private const int CantidadCaracteres = 23;
        private readonly ApplicationDbContext _context;
        private readonly IWorkflowService _wfService;
        private readonly ISolicitudRepository _solicitudRepository;
        public WorkflowScController(ApplicationDbContext context, IWorkflowService wfservice, ISolicitudRepository solicitudRepo)
        {
            _context = context;
            _wfService = wfservice;
            _solicitudRepository = solicitudRepo;
        }

        [Route("preparar-nomina-sc")]
        public IActionResult PreparaNominasc()
        {
            var comunaOficina = _context.Users.Include(us => us.Oficina).ThenInclude(of => of.Comuna).FirstOrDefault(usr => usr.UserName == User.Identity.Name).Oficina.Comuna;
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;

            return View();
        }

        [Route("despacho-a-custodia-sc")]
        public IActionResult DespachoCustodiaSc()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }

        [Route("despacho-oficina-evaluadora-sc")] 
        public IActionResult Despachooficinapagadorasc()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }



        [Route("recepcion-valija-documentos-sc")]
        public IActionResult RecepcionValijaDocumentosSc()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }


        [Route("recpecion-of-partes-sc")]
        public IActionResult recepcionofpartesSc()
        {
            var comunaOficina = _context.Users.Include(us => us.Oficina).ThenInclude(of => of.Comuna).FirstOrDefault(usr => usr.UserName == User.Identity.Name).Oficina.Comuna;
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;

            return View();
        }



        [Route("despacho-of-partes-sc")]
        public IActionResult DespachoOfpartesSc()
        {
            var comunaOficina = _context.Users.Include(us => us.Oficina).ThenInclude(of => of.Comuna).FirstOrDefault(usr => usr.UserName == User.Identity.Name).Oficina.Comuna;
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;

            return View();
        }

        [Route("despacho-a-suscursal-sc")]
        public IActionResult DespachoSucursalSc()
        {

            var salida = _solicitudRepository.listarSucursalespoEtapa();

            ViewBag.Sucursales = salida;

            return View();
        }


        [Route("apertura-valija-sc")]
        public IActionResult AperturaValijaSc()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }



        [Route("apertura-valija-documentos-sc")]
        public IActionResult AperturaValijaDocmuentosSc()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }


        [Route("recepcion-custodia-sc/{folioCaja?}")]
        public IActionResult RecepcionCustodiaSc(string folioCaja = "")
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            ViewBag.folioCaja = folioCaja;

            return View();
        }



        [Route("recepcion-sucursal-evaluadora-sc")]
        public IActionResult RecepcionSucursalEvaluadoraSc()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }


        [Route("recepcion-valija-Sc")]
        public IActionResult RecepcionValijaSc()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }

        [Route("analisis-mesa-control-Sc")]
        public IActionResult AnalisisMesaControlSc()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }

        [Route("solucion-expediente-faltante-setcomplementario")]
        public IActionResult SolucionExpedienteFaltanteSc()
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            return View();
        }



        [Route("notas-internas-Sc/generadas/{tipoDocumento}")]
        public IActionResult DocumentosGeneradosSc(string tipoDocumento)
        {
            ViewBag.CantidadCaracteresFolio = CantidadCaracteres;
            ViewBag.tipoDocumento = tipoDocumento;
            return View();
        }

        [Route("detalle-valija-valorada-Doc-Sucursal-sc/{codigoSeguimiento}")]
        public async Task<IActionResult> DetalleValijaValoradaDocSucursalSc([FromRoute] string codigoSeguimiento)
        {


            List<Oficina> ofingreso = new List<Oficina>();
            var oficinaEvaluadora = new List<CargaInicial>();
            var variable = new List<Variable>();

          
            var expcomp = new List<ExpedienteComplementario>();
            var valorada = new List<ValijaValorada>();
            var creditos = new List<Credito>();
            var excred = new List<ExpedienteCredito>();
            List<Documento> docu = new List<Documento>();
            expcomp = _context.ExpedientesComplementarios.Where(a => a.CodigoSeguimiento == codigoSeguimiento).ToList();

            foreach (var item in expcomp)
            {
                creditos.Add( await _context.Creditos.Where(a => a.Id == item.CreditoId).FirstOrDefaultAsync());
                valorada.Add(  _context.ValijasValoradas.Where(a => a.CodigoSeguimiento == item.CodigoSeguimiento && a.CodigoSeguimiento==codigoSeguimiento && a.MarcaAvance== "OF_PARTES_DOCUMENTOS").FirstOrDefault());

            }

            foreach (var item in creditos)
            {
                excred.Add(_context.ExpedientesCreditos.Where(a => a.CreditoId == item.Id && TipoExpediente.Complementario == a.TipoExpediente).FirstOrDefault());
            }
            

            foreach (var expdiente in excred)
            {
               
               var doc =_context.Documentos.Where(a => a.ExpedienteCredito.Id == expdiente.Id && a.Codificacion!="09" && a.Codificacion != "10").ToList();
               
            }



           

            var valija = _context.ValijasValoradas.Where(a=>a.CodigoSeguimiento==codigoSeguimiento).FirstOrDefault();

            valija.Expedientes = excred;

           
            
            

            ofingreso = _context.Oficinas.ToList();
            foreach (var credir in valija.Expedientes)
            {
                //var especiales = new Variable();
                //especiales = _context.Variables.Where(p => p.NumeroTicket == credir.Credito.NumeroTicket && p.Valor == "1" && p.Tipo == "string").FirstOrDefault();
                //if (especiales != null)
                //{
                //    variable.Add(especiales);
                //}
                var ofi = new List<Oficina>();
                var ci = _context.CargasIniciales.Where(a => a.FolioCredito == credir.Credito.FolioCredito).FirstOrDefault();
                oficinaEvaluadora.Add(ci);
               //var descripcionOficinaIngr = _context.Oficinas.Where(a => a.Codificacion == ci.CodigoOficinaIngreso).FirstOrDefault();
               // descripcionOficinaIngreso = descripcionOficinaIngr.ToString();
            }


            return new Rotativa.AspNetCore.ViewAsPdf(new PdfEnvioSucursalModelHelper()
            {
                FechaImpresion = valija.FechaEnvio.ToShortDateString(),
                CodigoSeguimiento = codigoSeguimiento,
                ValijaValorada = valija,
                OficinaEvaluadora = oficinaEvaluadora,
                OficinaPagadora = ofingreso

            })
            {
                PageSize = Size.Letter
            };
        }


        [Route("detalle-valija-valorada-sc/{codigoSeguimiento}")]
        public async Task<IActionResult> DetalleValijaValoradaSc([FromRoute] string codigoSeguimiento)
        {
            List<Oficina> ofingreso = new List<Oficina>();
            var oficinaEvaluadora = new List<CargaInicial>();
            var variable = new List<Variable>();
            var valorada = await _context.ValijasValoradas
                                .Include(pm => pm.Expedientes)
                                    .ThenInclude(e => e.Credito)
                                .Include(pm => pm.Expedientes)
                                    .ThenInclude(e => e.Documentos)
                                .Include(pm => pm.Oficina)
                                .FirstOrDefaultAsync(pn => pn.CodigoSeguimiento == codigoSeguimiento);

            ofingreso = _context.Oficinas.ToList();
            foreach (var credir in valorada.Expedientes)
            {
                var especiales = new Variable();
                especiales = _context.Variables.Where(p => p.NumeroTicket == credir.Credito.NumeroTicket && p.Valor == "1" && p.Tipo == "string").FirstOrDefault();
                if (especiales != null)
                {
                    variable.Add(especiales);
                }

                var ofi = new List<Oficina>();
               
                var ci = _context.CargasIniciales.Where(a => a.FolioCredito == credir.Credito.FolioCredito).FirstOrDefault();

                 oficinaEvaluadora.Add(ci);

               
            }


            return new Rotativa.AspNetCore.ViewAsPdf(new PdfModelHelper()
            {
                FechaImpresion = valorada.FechaEnvio.ToShortDateString(),
                CodigoSeguimiento = codigoSeguimiento,
                ValijaValorada = valorada,
                Variables = variable,
                OficinaEvaluadora = oficinaEvaluadora,
                OficinaPagadora= ofingreso
            })
            {
                PageSize = Size.Letter
            };
        }


    }
}