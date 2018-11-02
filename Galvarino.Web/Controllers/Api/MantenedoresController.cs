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
        public async Task<IActionResult> GuardarCambiosUsuario([FromBody] FormUsuario entrada)
        {
            Usuario elUsuario = await _userManager.FindByNameAsync(entrada.Identificacion);
            // String.IsNullOrEmpty(elUsuario.UserName)
            if (elUsuario == null)
            {
                var user = new Usuario
                {
                    UserName = entrada.Identificacion,
                    Email = entrada.Correo,
                    Identificador = entrada.Identificacion,
                    Nombres = entrada.Nombres,
                    PhoneNumber = entrada.Telefono,
                    Oficina = _context.Oficinas.Find(entrada.Oficina)
                };
                var result = await _userManager.CreateAsync(user, "Araucanos.789");

                if (result.Succeeded)
                {
                    var rs = await _userManager.AddToRolesAsync(user, entrada.Rol);

                    if (rs.Succeeded)
                    {
                        return Ok("Correcto!");
                    }
                    else
                    {
                        return BadRequest(rs.Errors);
                    }
                }
                else
                {
                    return BadRequest(result.Errors);
                }
            }
            else
            {
                elUsuario.UserName = entrada.Identificacion;
                elUsuario.Nombres = entrada.Nombres;
                elUsuario.Email = entrada.Correo;
                elUsuario.PhoneNumber = entrada.Telefono;
                elUsuario.Oficina = _context.Oficinas.Find(entrada.Oficina);

                var roledeslete = await _userManager.GetRolesAsync(elUsuario);
                if (roledeslete.Count > 0)
                {
                    var rs1 = await _userManager.RemoveFromRolesAsync(elUsuario, roledeslete);
                }
                var rs = await _userManager.AddToRolesAsync(elUsuario, entrada.Rol);

                var result = await _userManager.UpdateAsync(elUsuario);
                if (result.Succeeded)
                {
                    return Ok("Correcto!");
                }
                else
                {
                    return BadRequest(result.Errors);
                }
            }
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
        public async Task<IActionResult> EliminarRol([FromRoute] string identificador)
        {
            var role = await _roleManager.FindByIdAsync(identificador);
            await _roleManager.DeleteAsync(role);
            return Ok();
        }

        [HttpPost("roles")]
        public async Task<ActionResult> GuardarCambiosRol([FromBody] FormRol entrada)
        {
            var orga = _context.Organizaciones.Where(org => org.Id == entrada.Organizacion).FirstOrDefault();
            Rol elRol = _context.Roles.Find(entrada.Id);
            if (elRol != null)
            {

                elRol.Name = entrada.Nombre;
                elRol.Orzanizacion = orga;
                var result = await _roleManager.UpdateAsync(elRol);

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
                elRol = new Rol();
                elRol.Name = entrada.Nombre;
                elRol.Orzanizacion = orga;
                elRol.Activo = true;
                var result = await _roleManager.CreateAsync(elRol);

                if (result.Succeeded)
                {
                    return Ok("Correcto!");
                }
                else
                {
                    return BadRequest(result.Errors);
                }
            }
        }
        #endregion

        #region Workflow

        [HttpGet("workflow/procesos/{procesoId}/etapas/{etapaId}/transito")]
        public IActionResult DestinosEtapa([FromRoute] int etapaId)
        {
            return Ok(_context.Transiciones.Include(e => e.EtapaDestino).Where(t => t.EtapaActaual.Id == etapaId).ToList());
        }

        [HttpPost("workflow/procesos/{procesoId}/etapas")]
        public async Task<IActionResult> GardarEtapa([FromBody] EtapaHelper etapa)
        {

            if (etapa.id > 0)
            {
                Etapa etp = _context.Etapas.Include(f => f.Destinos).FirstOrDefault(x => x.Id == etapa.id);
                etp.Nombre = etapa.nombre;
                var ddd= '_';
                etp.NombreInterno = etapa.nombre.Replace(' ', ddd).ToUpper();
                etp.Proceso = _context.Procesos.Find(etapa.proceso);
                etp.Secuencia = 900;
                etp.TipoDuracion = (TipoDuracion)Enum.Parse(typeof(TipoDuracion), etapa.tipoDuracion);
                etp.ValorDuracion = etapa.valorDuracion;
                etp.TipoEtapa = (TipoEtapa)Enum.Parse(typeof(TipoEtapa), etapa.tipoEtapa);
                etp.TipoDuracionRetardo = (TipoDuracion)Enum.Parse(typeof(TipoDuracion), etapa.tipoDuracionRetardo);
                etp.ValorDuracionRetardo = etapa.valorDuracion;
                etp.TipoUsuarioAsignado = (TipoUsuarioAsignado)Enum.Parse(typeof(TipoUsuarioAsignado), etapa.tipoUsuarioAsignado);
                etp.ValorUsuarioAsignado = etapa.valorUsuarioAsignado;


                foreach (var destino in etapa.destinos)
                {
                    if (destino.id > 0)
                    {
                        var transito = _context.Transiciones.Find(destino.id);
                        transito.NamespaceValidacion = destino.namespaceValidacion;
                        transito.ClaseValidacion = destino.claseValidacion;
                        transito.MetodoValidacion = destino.metodoValidacion;
                        transito.EtapaDestino = _context.Etapas.Find(destino.etapaDestino);
                        transito.EtapaActaual = etp;
                    }
                    else
                    {
                        _context.Transiciones.Add(new Transito
                        {
                            NamespaceValidacion = destino.namespaceValidacion,
                            ClaseValidacion = destino.claseValidacion,
                            MetodoValidacion = destino.metodoValidacion,
                            EtapaDestino = _context.Etapas.Find(destino.etapaDestino),
                            EtapaActaual = etp
                        });
                    }
                }
                _context.Etapas.Update(etp);

            }
            else
            {
                Etapa etp = new Etapa();
                etp.Nombre = etapa.nombre;
                etp.NombreInterno = etapa.nombre.Replace(' ', '_').ToUpper();
                etp.Proceso = _context.Procesos.Find(etapa.proceso);
                etp.Secuencia = 900;
                etp.TipoDuracion = (TipoDuracion)Enum.Parse(typeof(TipoDuracion), etapa.tipoDuracion);
                etp.ValorDuracion = etapa.valorDuracion;
                etp.TipoEtapa = (TipoEtapa)Enum.Parse(typeof(TipoEtapa), etapa.tipoEtapa);
                etp.TipoDuracionRetardo = (TipoDuracion)Enum.Parse(typeof(TipoDuracion), etapa.tipoDuracionRetardo);
                etp.ValorDuracionRetardo = etapa.valorDuracion;
                etp.TipoUsuarioAsignado = (TipoUsuarioAsignado)Enum.Parse(typeof(TipoUsuarioAsignado), etapa.tipoUsuarioAsignado);
                etp.ValorUsuarioAsignado = etapa.valorUsuarioAsignado;

                foreach (var destino in etapa.destinos)
                {
                    etp.Destinos.Add(new Transito
                    {
                        NamespaceValidacion = destino.namespaceValidacion,
                        ClaseValidacion = destino.claseValidacion,
                        MetodoValidacion = destino.metodoValidacion,
                        EtapaDestino = _context.Etapas.Find(destino.etapaDestino)
                    });
                }
                _context.Etapas.Add(etp);
            }

            await _context.SaveChangesAsync();

            return Ok(etapa);
        }

        #endregion

    }
}
