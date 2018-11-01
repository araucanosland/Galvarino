using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Galvarino.Web.Models;
using Galvarino.Web.Services.Workflow;
using Galvarino.Web.Models.Helper;
using Galvarino.Web.Data;
using Microsoft.EntityFrameworkCore;
using Galvarino.Web.Models.Application;
using Galvarino.Web.Models.Workflow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using Galvarino.Web.Models.Security;

namespace Galvarino.Web.Controllers.Api
{
    [Route("api/mantenedores")]
    [ApiController]
    [Authorize]
    public class MantenedoresController : ControllerBase
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<Rol> _roleManager;
        private readonly ApplicationDbContext _context;
        public MantenedoresController(
                UserManager<Usuario> userManager,
                RoleManager<Rol> roleManager,
                IConfiguration configuration,
                ApplicationDbContext context)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }


        #region Usuarios Sistema

        [HttpGet("usuarios")]
        public async Task<IActionResult> ListarUsuarios()
        {
            return Ok();
        } 

        [HttpGet("usuarios/{identificador}")]
        public IActionResult ObtenerUsuario([FromRoute] string identificador)
        {
            return Ok();
        }

        [HttpDelete("usuarios/{identificador}")]
        public IActionResult EliminarUsuario([FromRoute] string identificador)
        {
            return Ok();
        }

        [HttpPost("usuarios")]
        public IActionResult GuardarCambiosUsuario([FromBody] InfoUsuario model)
        {
            return Ok();
        }


        #endregion

        #region Roles Sistema
        [HttpGet("roles")]
        public async Task<IActionResult> ListarRoles()
        {
            return Ok();
        }

        [HttpGet("roles/{identificador}")]
        public IActionResult ObtenerRol([FromRoute] string identificador)
        {
            return Ok();
        }

        [HttpDelete("roles/{identificador}")]
        public IActionResult EliminarRol([FromRoute] string identificador)
        {
            return Ok();
        }

        [HttpPost("roles")]
        public IActionResult GuardarCambiosRol([FromBody] InfoUsuario model)
        {
            return Ok();
        }
        #endregion

        

    }
}
