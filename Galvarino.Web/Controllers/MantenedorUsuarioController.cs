using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Galvarino.Web.Data;
using Microsoft.AspNetCore.Identity;
using Galvarino.Web.Models.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Galvarino.Web.Controllers
{
    [Route("configuraciones/usuarios")]
    [Authorize]
    public class MantenedorUsuarioController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<Rol> _roleManager;
        public MantenedorUsuarioController(ApplicationDbContext context,
                                            RoleManager<Rol> roleManager,
                                            UserManager<Usuario> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }
        
        [Route("")]
        public IActionResult Listado()
        {
            ViewBag.usuariosList = _context.Users.ToList();
            return View();
        }

        [Route("formulario/{id?}")]
        public async Task<IActionResult> Formulario(string id="")
        {

            ViewBag.editando = !String.IsNullOrEmpty(id);
            ViewBag.rolesList = _context.Roles.Include(r => r.Orzanizacion).ToList();
            ViewBag.oficinasList = _context.Oficinas.OrderBy(ofc => ofc.Codificacion).ToList();
            if (ViewBag.editando)
            {
                var user = await _context.Users.FindAsync(id);
                ViewBag.rolUsuario = await _userManager.GetRolesAsync(user);
                ViewBag.usuario = user;
            }

            return View();
        }

        [HttpGet("carga-masiva")]
        public IActionResult CargaMasiva()
        {
            return View();
        }

        [HttpPost("carga-masiva")]
        public async Task<IActionResult> CargaMasiva(IFormFile file)
        {
            if(file == null || file.Length == 0)
                return Content("file not selected");

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.Name);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            foreach (var line in System.IO.File.ReadLines(path))
            {
                string[] fields = line.Split(new char[] { ';' });

                var user = new Usuario
                {
                    UserName = fields[0],
                    Email = fields[2],
                    Identificador = fields[0],
                    Nombres = fields[1],
                    Oficina = _context.Oficinas.FirstOrDefault(ofi => ofi.Codificacion == fields[3])
                };
                var result = await _userManager.CreateAsync(user, "Araucanos.789");
                if (result.Succeeded)
                {
                    var rs = await _userManager.AddToRoleAsync(user, fields[4]);
                }
            }
            

            return View();

        }
    }
}