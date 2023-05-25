using Galvarino.Web.Data;
using Galvarino.Web.Models.Application;
using Galvarino.Web.Models.Application.Pensionado;
using Galvarino.Web.Models.Security;
using Galvarino.Web.Models.Workflow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Galvarino.Web.Controllers.Pensionados
{
    [Route("Pensionado")]
    public class BusquedaController : Controller
    {
        private readonly PensionadoDbContext _context;
        private readonly ApplicationDbContext _contextApp;

        public BusquedaController(PensionadoDbContext context, ApplicationDbContext contextApp)
        {
            _context = context;
            _contextApp = contextApp;
        }

        [Route("busqueda-pensionado")]
        public IActionResult BusquedaPensionado()
        {
            return View();
        }

        [Route("resultado-busqueda/{folio}")]
        public IActionResult ResultadoBusquedaPensionado(string folio)
        {
            try
            {
                var pensionado = _context.Pensionado.Where(p => p.Folio == folio.ToString()).FirstOrDefault();
                var tareas = _context.Tareas.Include(t => t.Solicitudes).Include(e => e.Etapas).Where(t => t.Solicitudes.NumeroTicket == pensionado.NumeroTicket).ToList();
                var etapas = _context.Etapas.ToList();
                var tipo = _context.Tipo.Where(t => t.Id == pensionado.TipoPensionado).FirstOrDefault();

                return View(new ModeloBusqueda
                {
                    Pensionado = pensionado,
                    etapas = etapas,
                    tipo = tipo,
                    Tareas = tareas,
                    Usuarios = _contextApp.Users.Include(u => u.Oficina).ToList(),

                });


            }
            catch (Exception ex)
            {
                return View("NoEncontradoPensionado");
            }

        }
    }

    public class ModeloBusqueda
    {
        public Pensionado Pensionado { get; set; }
        public Etapas Etapas { get; set; }
        public Tipo tipo { get; set; }
        public IEnumerable<Tareas> Tareas { get; set; }
        public IEnumerable<Etapas> etapas { get; set; }
        public IEnumerable<Usuario> Usuarios { get; set; }


    }

}
