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
    }
}