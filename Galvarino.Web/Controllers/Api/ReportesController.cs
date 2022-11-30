using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Galvarino.Web.Data;
using Galvarino.Web.Data.Repository;
using Galvarino.Web.Models.Application;
using Galvarino.Web.Models.Helper;
using Galvarino.Web.Models.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Galvarino.Web.Controllers.Api
{
    [Route("api/reportes")]
    [ApiController]
    public class ReportesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ISolicitudRepository _solicitudRepository;

        public ReportesController(ISolicitudRepository solicitudRepository,ApplicationDbContext context)
        {
            _context = context;
            _solicitudRepository = solicitudRepository;
        }

        [HttpGet("Preparar-Nomina/{etapaIn}")]
        public async  Task<IActionResult> ReportePrepararNomina([FromRoute] string etapaIn = "")
        {
            
            string fechaCerrar = "";
            string notaria = "";
            
            var rolesUsuario = User.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToArray();
            var oficinaUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
            var ObjetoOficinaUsuario = await _context.Oficinas.Include(of => of.OficinaProceso).FirstOrDefaultAsync(ofc => ofc.Codificacion == oficinaUsuario);
            var OficinasUsuario = _context.Oficinas.Where(ofc => ofc.OficinaProceso.Id == ObjetoOficinaUsuario.Id && ofc.EsMovil == true);
            var ofinales = new List<Oficina>();
            ofinales.Add(ObjetoOficinaUsuario);
            ofinales.AddRange(OficinasUsuario.ToList());
            string orden = "1";
            var lasOff = ofinales.Select(x => x.Codificacion).ToArray();
            var lasEtps = new string[] { etapaIn };
            var salida = _solicitudRepository.listarSolicitudesReportes(rolesUsuario, User.Identity.Name, lasOff, lasEtps, orden, fechaCerrar, notaria);
           

            DataTable data = Newtonsoft.Json.JsonConvert.DeserializeObject<DataTable>(Newtonsoft.Json.JsonConvert.SerializeObject(salida.ToList()));
            
            using (var libro = new XLWorkbook())
            {
                data.TableName = "Prep_Nomina_" + DateTime.Now.ToString("M");
                var hoja = libro.Worksheets.Add(data);
                hoja.ColumnsUsed().AdjustToContents();

                using (var memoria = new MemoryStream())
                {
                    libro.SaveAs(memoria);
                    var nombreExcel = "Prep_Nomina_" + DateTime.Now.ToString("M") + ".xlsx";
                    return File(memoria.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreExcel);
                }
            }
        }

        [HttpGet("Envio-Notaria/{etapaIn}")]
        public async Task<IActionResult> ReporteEnvioaNotaria([FromRoute] string etapaIn = "")
        {
            string fechaCerrar = "";
            string notaria = "";

            var rolesUsuario = User.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToArray();
            var oficinaUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
            var ObjetoOficinaUsuario = await _context.Oficinas.Include(of => of.OficinaProceso).FirstOrDefaultAsync(ofc => ofc.Codificacion == oficinaUsuario);
            var OficinasUsuario = _context.Oficinas.Where(ofc => ofc.OficinaProceso.Id == ObjetoOficinaUsuario.Id && ofc.EsMovil == true);
            var ofinales = new List<Oficina>();
            ofinales.Add(ObjetoOficinaUsuario);
            ofinales.AddRange(OficinasUsuario.ToList());
            string orden = "1";
            var lasOff = ofinales.Select(x => x.Codificacion).ToArray();
            var lasEtps = new string[] { etapaIn };
            var salida = _solicitudRepository.listarSolicitudesReportes(rolesUsuario, User.Identity.Name, lasOff, lasEtps, orden, fechaCerrar, notaria);


            DataTable data = Newtonsoft.Json.JsonConvert.DeserializeObject<DataTable>(Newtonsoft.Json.JsonConvert.SerializeObject(salida.ToList()));

            using (var libro = new XLWorkbook())
            {
                data.TableName = "Env_Notaria_" + DateTime.Now.ToString("M");
                var hoja = libro.Worksheets.Add(data);
                hoja.ColumnsUsed().AdjustToContents();

                using (var memoria = new MemoryStream())
                {
                    libro.SaveAs(memoria);
                    var nombreExcel = "Env_Notaria_" + DateTime.Now.ToString("M") + ".xlsx";
                    return File(memoria.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreExcel);
                }
            }
        }

        [HttpGet("Despacho-Of-Partes/{etapaIn}")]
        public async Task<IActionResult> ReporteDespachoOfPartes([FromRoute] string etapaIn = "")
        {
            string fechaCerrar = "";
            string notaria = "";

            var rolesUsuario = User.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToArray();
            var oficinaUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
            var ObjetoOficinaUsuario = await _context.Oficinas.Include(of => of.OficinaProceso).FirstOrDefaultAsync(ofc => ofc.Codificacion == oficinaUsuario);
            var OficinasUsuario = await _context.Oficinas.Where(ofc => ofc.OficinaProceso.Id == ObjetoOficinaUsuario.Id && ofc.EsMovil == true).ToListAsync();
            var ofinales = new List<Oficina>();
            ofinales.Add(ObjetoOficinaUsuario);
            ofinales.AddRange(OficinasUsuario.ToList());
            string orden = "1";
            var lasOff = ofinales.Select(x => x.Codificacion).ToArray();
            var lasEtps = new string[] { etapaIn };
            var salida = _solicitudRepository.listarSolicitudesReportes(rolesUsuario, User.Identity.Name, lasOff, lasEtps, orden, fechaCerrar, notaria);


            DataTable data = Newtonsoft.Json.JsonConvert.DeserializeObject<DataTable>(Newtonsoft.Json.JsonConvert.SerializeObject(salida.ToList()));


                using (var libro = new XLWorkbook())
                {
                    data.TableName = "Des_Of_Partes_" + DateTime.Now.ToString("M");
                    var hoja = libro.Worksheets.Add(data);
                    hoja.ColumnsUsed().AdjustToContents();

                    using (var memoria = new MemoryStream())
                    {
                        libro.SaveAs(memoria);
                        var nombreExcel = "Des_Of_Partes_" + DateTime.Now.ToString("M") + ".xlsx";
                        return File(memoria.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreExcel);
                    }
                }
           
        }

    }
}