using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Galvarino.Web.Models;
using Galvarino.Web.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Galvarino.Web.Data;
using Microsoft.AspNetCore.Authorization;

namespace Galvarino.Web.Controllers.Api
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<Rol> _roleManager;
        private readonly ApplicationDbContext _context;

        public AuthController(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager,
            RoleManager<Rol> roleManager,
            ApplicationDbContext context,
            IConfiguration configuration
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _context = context;
        }
       
        [HttpPost]
        [Route("challenge")]
        public async Task<IActionResult> Autehticate([FromBody] InfoUsuario infoUsuario)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(infoUsuario.Identificacion, infoUsuario.Llave, isPersistent: true, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return Ok();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Datos de Acceso Invalidos.");
                    return BadRequest(ModelState);
                }
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        
        [Route("user-create")]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] InfoUsuario model)
        {

            if (ModelState.IsValid)
            {
                if(model.Firma == "CARLITOS%EL%MAS%BONITO")
                {
                    var laofi = _context.Oficinas.FirstOrDefault(of => of.Codificacion == model.CodificacionOficina);
                    var user = new Usuario
                    {
                        UserName = model.Identificacion,
                        Email = model.Correo,
                        Identificador = model.Identificacion,
                        Nombres = model.Nombres,
                        Oficina = laofi
                    };

                    var result = await _userManager.CreateAsync(user, "Araucanos.789");
                    if (result.Succeeded)
                    {
                        
                        var existeRol = _roleManager.Roles.FirstOrDefault(x => x.NormalizedName == model.Rol.ToUpper());
                        if(existeRol == null)
                        {   
                            /*La Araucana Org */
                            var laOrg = _context.Organizaciones.Find(1);
                            existeRol = new Rol();
                            existeRol.Activo = true;
                            existeRol.Name = model.Rol;
                            existeRol.Orzanizacion = laOrg;
                            await _roleManager.CreateAsync(existeRol);
                        }

                        await _userManager.AddToRoleAsync(user, model.Rol);
                        return Ok();
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Algo anda mal al crear el Usuario.");
                        return BadRequest(ModelState);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Firma sin autorizacion o caducada.");
                    return BadRequest(ModelState);
                }   
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpGet("offices-list")]
        public IActionResult ListOffices()
        {
            return Ok(_context.Oficinas.ToList());
        }

        [Route("role-create")]
        [HttpPost]
        public async Task<IActionResult> CreaRol([FromBody] InfoRol model)
        {
            if (ModelState.IsValid)
            {
                var orga = _context.Organizaciones.Where(org => org.Id == model.OrganizacionId).FirstOrDefault();

                //Padre = !string.IsNullOrEmpty(model.PadreId) ? _context.Roles.Where(role => role.Id == model.PadreId).FirstOrDefault() : null,
                var rl = new Rol
                {
                    Name = model.Nombre,
                    Orzanizacion = orga,
                    Activo = true
                };

                var result = await _roleManager.CreateAsync(rl);

                if (result.Succeeded)
                {
                    return Ok("Correcto!");
                }
                else
                {
                    return BadRequest(result.Errors);
                }
            }
            else
            {
                return BadRequest("Algo anda mal");
            }
        }
        
    }
}
