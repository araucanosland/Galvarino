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
using Galvarino.Web.Services.Workflow;
using System.Security.Claims;
using Galvarino.Web.Models.Security;
using Galvarino.Web.Data.Repository;
using Galvarino.Web.Models.Mappings;

namespace Galvarino.Web.Controllers
{
    [Route("reportes")]
    [Authorize]
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWorkflowService _wfService;
        private readonly ISolicitudRepository _solicitudRepository;
        public ReportesController(ApplicationDbContext context, IWorkflowService wfservice, ISolicitudRepository solicitudRepo)
        {
            _context = context;
            _wfService = wfservice;
             _solicitudRepository = solicitudRepo;
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


        [Route("workflow/mesa-control")]
        public IActionResult ReporteMesaControl()
        {
            return View();
        }


        [Route("workflow/movilidad-documentos")]
        public IActionResult MovilidadDocumentos()
        {

            var salida = new List<dynamic>();
            var oficinas =  _context.Oficinas.Include(d => d.OficinaProceso).Where(ofi => ofi.Id == ofi.OficinaProceso.Id);
            var tareas = _context.Tareas.Include(d => d.Etapa).Include(f => f.Solicitud).Where(x => x.Estado == EstadoTarea.Activada); 
            var expedientes = _context.ExpedientesCreditos
                                .Include(f => f.Documentos)
                                .Include(f => f.Credito);

            var offices = from off in oficinas 
                         join tar in tareas on off.Codificacion equals tar.UnidadNegocioAsignada
                         join exp in expedientes on tar.Solicitud.NumeroTicket equals exp.Credito.NumeroTicket
                         group new {off, tar, exp} by new {Oficina = off, Tarea = tar, Expediente = exp} into grp
                         select new {
                            Container = grp.Key,
                            Total = grp.Count()
                         };
                         
                         
                         /*EstructuraMovilidad
                         {
                             Oficina = off,
                             Tarea = tar,
                             Expediente = exp
                         };*/
                         

            
            return Json(offices);
        }




        [HttpGet("reporte-setcomplementario/{etapaIn?}/{fechaCerrar?}")]
        public IActionResult Listarreporte([FromRoute] string etapaIn = "", [FromRoute] string fechaCerrar = "", [FromQuery] int offset = 0, [FromQuery] int limit = 20, [FromQuery] string sort = "", [FromQuery] string order = "", [FromQuery] string sucursal = "")
        {


            //var rolUsuario =  //FirstOrDefault(x => x.Type == ClaimTypes.Role).Value;    

            var rolesUsuario = User.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToArray();
            var oficinaUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
            var ObjetoOficinaUsuario = _context.Oficinas.Include(of => of.OficinaProceso).FirstOrDefault(ofc => ofc.Codificacion == oficinaUsuario);
            var OficinasUsuario = _context.Oficinas.Where(ofc => ofc.OficinaProceso.Id == ObjetoOficinaUsuario.Id && ofc.EsMovil == true);
            var ofinales = new List<Oficina>();
            ofinales.Add(ObjetoOficinaUsuario);
            ofinales.AddRange(OficinasUsuario.ToList());

            var lasOff = ofinales.Select(x => x.Codificacion).ToArray();
            var lasEtps = new string[] { etapaIn };
            string orden = sort + " " + order;
            var salida = _solicitudRepository.listarSolicitudesSc(rolesUsuario, User.Identity.Name, lasOff, lasEtps, orden, fechaCerrar, sucursal);

          
            return Json(salida);

          
        }


    }

    public class EstructuraMovilidad{
        public Oficina Oficina { get; set; }
        public Tarea Tarea { get; set; }
        public ExpedienteCredito Expediente { get; set; }
    }
}