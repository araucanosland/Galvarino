using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Galvarino.Web.Data;
using Microsoft.EntityFrameworkCore;
using Galvarino.Web.Models.Helper;
using Galvarino.Web.Models.Application;

namespace Galvarino.Web.Controllers
{
    [Route("configuraciones/notarias")]
    [Authorize]
    public class MantenedorNotariasController : Controller
    {

        private readonly ApplicationDbContext _context;
        public MantenedorNotariasController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("")]
        public IActionResult Listado()
        {
            var notarias = _context.Notarias.Include(ntr => ntr.Comuna).ThenInclude(com => com.Region).OrderBy(not => not.Comuna.Region.Secuencia).ToList();
            ViewBag.notariasList = notarias;
            return View();
        }

        [Route("formulario/{id?}")]
        public IActionResult Formulario(string id = "")
        {
            if(!String.IsNullOrEmpty(id))
            {
                var notaria = _context.Notarias.Find(Convert.ToInt32(id));
                ViewBag.notaria = notaria;
            }
            
            ViewBag.editando = !String.IsNullOrEmpty(id);
            ViewBag.regionesList = _context.Regiones.Include(reg=> reg.Comunas).ToList();
            return View();
        }


        [Route("localizacion/regiones/{region}/comunas")]
        public IActionResult Comunas(int region)
        {
            return Ok(_context.Comunas.Include(cm => cm.Region).Where(comu => comu.Region.Id == region));
        }

        [HttpPost("crud")]
        public IActionResult SaveNotaria([FromBody] FormNotaria entrada)
        {
            if (entrada == null)
            {
                throw new ArgumentNullException(nameof(entrada));
            }

            Notaria notari;
            if(!string.IsNullOrEmpty(entrada.Id) && entrada.Id != null && Convert.ToInt32(entrada.Id) > 0)
            {
                notari = _context.Notarias.Find(Convert.ToInt32(entrada.Id));
                notari.Nombre = entrada.Nombre;
                notari.Comuna = _context.Comunas.Find(Convert.ToInt32(entrada.Comuna));
                _context.Notarias.Update(notari);

            }else{
                notari = new Notaria
                {
                    Nombre = entrada.Nombre,
                    Comuna = _context.Comunas.Find(Convert.ToInt32(entrada.Comuna))
                };
                _context.Notarias.Add(notari);
            }
            
            _context.SaveChanges();

            return Ok();
        }
    }
}