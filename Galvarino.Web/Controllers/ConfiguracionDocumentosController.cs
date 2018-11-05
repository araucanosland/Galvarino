using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Galvarino.Web.Data;
using Microsoft.EntityFrameworkCore;
using Galvarino.Web.Models.Application;
using Galvarino.Web.Models.Helper;

namespace Galvarino.Web.Controllers
{
    [Route("configuraciones/documentos")]
    [Authorize]
    public class ConfiguracionDocumentosController : Controller
    {

        private readonly ApplicationDbContext _context;
        public ConfiguracionDocumentosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("")]
        public IActionResult Listado()
        {
            ViewBag.configDocs = _context.ConfiguracionDocumentos.ToList();
            return View();
        }

        [Route("formulario/{id?}")]
        public IActionResult Formulario(string id = "")
        {
            var editando = !string.IsNullOrEmpty(id);

            ViewBag.editando = editando;
            if(editando)
            {
                ViewBag.configDocumento = _context.ConfiguracionDocumentos.Find(Convert.ToInt32(id));
            }
            
            ViewBag.tipoDocumento = Enum.GetValues(typeof(TipoDocumento))
                .Cast<TipoDocumento>()
                .Select(x => new DefaultClaveValor{ valor = x.ToString(), descripcion = System.Text.RegularExpressions.Regex.Replace(x.ToString(), "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim() })
                .ToArray();

            ViewBag.tipoExpediente = Enum.GetValues(typeof(TipoExpediente))
                .Cast<TipoExpediente>()
                .Select(x => new DefaultClaveValor{ valor = x.ToString(), descripcion = System.Text.RegularExpressions.Regex.Replace(x.ToString(), "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim() })
                .ToArray();

            ViewBag.tipoCredito = Enum.GetValues(typeof(TipoCredito))
                .Cast<TipoCredito>()
                .Select(x => new DefaultClaveValor{ valor = x.ToString(), descripcion = System.Text.RegularExpressions.Regex.Replace(x.ToString(), "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim() })
                .ToArray();
            
            return View();
        }
    }
}