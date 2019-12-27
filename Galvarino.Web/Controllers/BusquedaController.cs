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
using Galvarino.Web.Models.Security;

namespace Galvarino.Web.Controllers
{
    [Route("busqueda")]
    [Authorize]
    public class BusquedaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWorkflowService _wfService;
        public BusquedaController(ApplicationDbContext context, IWorkflowService wfservice)
        {
            _context = context;
            _wfService = wfservice;
        }

        [Route("resultado-busqueda/{folioCredito}")]
        public IActionResult ResultadoBusqueda(string folioCredito)
        {
            try{
                var credito = _context.Creditos.FirstOrDefault(cred => cred.FolioCredito == folioCredito);
                var solicitud = _context.Solicitudes.Include(sol => sol.Tareas).ThenInclude(tar => tar.Etapa).FirstOrDefault(sol => sol.NumeroTicket == credito.NumeroTicket);
                var oficinaComercial = _context.Oficinas.FirstOrDefault(o => o.Codificacion == _wfService.ObtenerVariable("OFICINA_INGRESO", credito.NumeroTicket));
                var oficinaLegal = _context.Oficinas.FirstOrDefault(o => o.Codificacion == _wfService.ObtenerVariable("OFICINA_PAGO", credito.NumeroTicket));
                var oficinaLegalizacion = _context.Oficinas.FirstOrDefault(l => l.Codificacion == _wfService.ObtenerVariable("OFICINA_PROCESA_NOTARIA", credito.NumeroTicket));
                
                return View(new ModeloBusqueda
                {
                    Credito = credito,
                    Solicitud = solicitud,
                    OficinaComercial = oficinaComercial,
                    OficinaLegal = oficinaLegal,
                    OficinaLegalizacion = oficinaLegalizacion,
                    Usuarios = _context.Users.Include(u => u.Oficina).ToList()
                });
            }catch(Exception ex)
            {
                return View("NoEncontrado");
            }
            
        }
    }

    public class ModeloBusqueda
    {
        public Credito Credito { get; set; }
        public Solicitud Solicitud { get; set; }
        public Oficina OficinaComercial { get; set; }
        public Oficina OficinaLegal { get; set; }
        public Oficina OficinaLegalizacion { get; set; }
        public IEnumerable<Usuario> Usuarios { get; set; }
        
    }
}