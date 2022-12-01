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
using Microsoft.Extensions.Configuration;

namespace Galvarino.Web.Controllers.Api
{
    [Route("api/reportes")]
    [ApiController]
    public class ReportesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ISolicitudRepository _solicitudRepository;
        private readonly IConfiguration _configuration;

        public ReportesController(IConfiguration configuration, ISolicitudRepository solicitudRepository,ApplicationDbContext context)
        {
            _context = context;
            _solicitudRepository = solicitudRepository;
            _configuration = configuration;
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

        [HttpGet("Lista-Registros-Reportes")]
        public ActionResult ListaRegistroReportes([FromQuery] int offset = 0, [FromQuery] int limit = 20)
        {
            var listaRegistroReportes = _solicitudRepository.ListaRegistroReporteProgramado();

            var lida = new
            {
                total = listaRegistroReportes.Count(),
                rows = listaRegistroReportes.Skip(offset).Take(limit).ToList()
            };



            //var reporte = await _context.ReporteProgramado.ToListAsync();
            return Ok(lida);
        }

        [HttpPost("Crear-Fecha-reporte-programado")]
        public async Task<IActionResult> CreaFechaReportesProgramado([FromBody] dynamic data)
        {
            try
            {
                string inicio = data.FechaInicial;
                string final = data.FechaFinal;
                string ejecucion = data.FechaEjecucion;

                DateTime fechainicial = Convert.ToDateTime(inicio);
                DateTime fechafinal = Convert.ToDateTime(final);
                DateTime fechaEjecucion = Convert.ToDateTime(ejecucion);

                var rutUsuario = User.Claims.Where(x => x.Type == ClaimTypes.Name).Select(x => x.Value).FirstOrDefault();
                var nombreUsuario = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.UsuarioNombres).Value;

                //no puede generar mas de un reporte con el mismo dia de ejecucion
                var reporte = await _context.ReporteProgramado.Where(x => x.Estado == "Pendiente" && x.RutUsuario == rutUsuario && x.FechaEjecucion == fechaEjecucion).FirstOrDefaultAsync();

                if (reporte != null)
                {
                    return BadRequest();
                }

                ReporteProgramado repo = new ReporteProgramado();
                repo.FechaInicio = fechainicial;
                repo.FechaFinal = fechafinal;
                repo.FechaEjecucion = fechaEjecucion;
                repo.Estado = "Pendiente";
                repo.RutUsuario = rutUsuario;
                repo.NombreUsuario = nombreUsuario;

                _context.ReporteProgramado.Add(repo);
                await _context.SaveChangesAsync();

                //var creaRegistro = _solicitudRepository.CreaRegistroReporteProgramado(rutUsuario.ToString(), fechainicial, fechafinal, fechaActual);

                //var salida = _solicitudRepository.ReporteGestion(fechainicial, fechafinal);
                ResultadoBase resultado = new ResultadoBase();
                resultado.Estado = "OK";
                resultado.Mensaje = "Se agenda reporte programado...<br/><small>Correctamente!!!</small>";
                resultado.Objeto = "";

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                ResultadoBase resultado = new ResultadoBase();
                resultado.Estado = "Error";
                resultado.Mensaje = ex.Message;
                resultado.Objeto = "";

                return Ok(resultado);
            }

        }

        [HttpGet("Exportar-Reporte-Programado")]
        public async Task<IActionResult> ExportarReportesProgramado()
        {
            try
            {
                string fecha = DateTime.Now.ToString("dd-MM-yyyy");
                DateTime fechaActual = Convert.ToDateTime(fecha);


                //revisar y modificar para que traiga el ultimo registro no solo el activo
                List<ReporteProgramado> reporte = await _context.ReporteProgramado.Where(x => x.Estado == "Pendiente" && x.FechaEjecucion == fechaActual).ToListAsync();

                if (reporte.Count() == 0)
                {
                    ResultadoBase respuesta = new ResultadoBase();
                    respuesta.Estado = "Ok";
                    respuesta.Mensaje = "No existen Reportes Pendientes";
                    respuesta.Objeto = "";
                    return Ok(respuesta);
                }

                foreach (ReporteProgramado rep in reporte)
                {

                    string nombreArchivo = "REPORTE_CREDITOS_GALVARINO" + "_" + rep.RutUsuario + ".xlsx";
                    string ruta = _configuration["RutaCargaReporteProgramado"];
                    string rutacompleta = ruta + nombreArchivo;
                    if (!Directory.Exists(ruta))
                        Directory.CreateDirectory(ruta);

                    if (System.IO.File.Exists(rutacompleta))
                        System.IO.File.Delete(rutacompleta);

                    DateTime fechainicial = Convert.ToDateTime(rep.FechaInicio);
                    DateTime fechafinal = Convert.ToDateTime(rep.FechaFinal);

                    DataTable data = _solicitudRepository.ObtenerDataReporte(fechainicial, fechafinal);

                    //var salida = _solicitudRepository.ReporteGestion(fechainicial, fechafinal);

                    using (var libro = new XLWorkbook())
                    {
                        data.TableName = "Reporte_" + DateTime.Now.ToString("M");
                        var hoja = libro.Worksheets.Add(data);
                        hoja.ColumnsUsed().AdjustToContents();

                        using (var memoria = new MemoryStream())
                        {
                            //libro.SaveAs(memoria);
                            libro.SaveAs(rutacompleta);
                            //var nombreExcel = "REPORTE_CREDITOS_GALVARINO.xlsx";
                            //return File(memoria.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreExcel);
                        }
                    }
                    rep.Estado = "Finalizado";
                    _context.ReporteProgramado.Update(rep);
                    await _context.SaveChangesAsync();
                }
                ResultadoBase resultado = new ResultadoBase();
                resultado.Estado = "Ok";
                resultado.Mensaje = "Reporte descargado en ruta";
                resultado.Objeto = "";
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                ResultadoBase resultado = new ResultadoBase();
                resultado.Estado = "Error";
                resultado.Mensaje = ex.Message;
                resultado.Objeto = "";

                return Ok(resultado);
            }
        }

    }
}