using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Galvarino.Web.Models.Application;
using Microsoft.AspNetCore.Identity;
using Galvarino.Web.Models.Security;
using Microsoft.Extensions.Configuration;
using Galvarino.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Galvarino.Web.Controllers
{
    [Route("legacy")]
    public class LegacyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LegacyController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("tubo-pagares-im")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("tubo-pagares-im/busqueda/{folioCredito}")]
        public IActionResult BusquedaPorFolio(string folioCredito)
        {
            var elPagare = _context.PagaresSinCustodia.FirstOrDefault(pagare => pagare.FolioCreditoStr == folioCredito);
            var lasGestion = _context.GestionPagaresSinCustodia.Include(pa => pa.PagareSinCustodia).Where(pag => pag.PagareSinCustodia == elPagare);

            var salida = new {
                pagare = elPagare,
                gestiones = lasGestion
            };

            return Ok(salida);
        }

        [HttpPost("tubo-pagares-im")]
        public async Task<IActionResult> GuardaGestion([FromBody] dynamic postData)
        {
            string elFolio = postData.folioCredito;
            string elEstado = postData.nuevoEstado;
            PagareSinCustodia ps = _context.PagaresSinCustodia.FirstOrDefault(pag => pag.FolioCreditoStr == elFolio);
            GestionPagareSinCustodia gsc = new GestionPagareSinCustodia
            {
                EjecutadoPor = User.Identity.Name,
                FechaGestion = DateTime.Now,
                Id = Guid.NewGuid().ToString(),
                PagareSinCustodia = ps,
                Estado = elEstado
            };

            _context.GestionPagaresSinCustodia.Add(gsc);
            await _context.SaveChangesAsync();
            return Ok(postData);
        }
    }
}
