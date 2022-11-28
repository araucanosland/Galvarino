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
using Galvarino.Web.Models.Security;
using Galvarino.Web.Data.Repository;
using System.Data;
using ClosedXML.Excel;
using System.IO;
using System.Reflection;

namespace Galvarino.Web.Controllers.Api
{
    [Route("api/app/v1")]
    [ApiController]
    [Authorize]
    public class BusinessController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly ISolicitudRepository _solicitudRepository;
        public BusinessController(ApplicationDbContext context, ISolicitudRepository solicitudRepo)
        {
            _context = context;
            _solicitudRepository = solicitudRepo;
        }

        /*Deprecated */
        [HttpGet("obtener-expediente/{folioCredito}")]
        public IActionResult ObtenerXpediente([FromRoute] string folioCredito)
        {

            var expediente = _context.ExpedientesCreditos.Include(x => x.Credito).Include(x => x.Documentos).FirstOrDefault(x => x.Credito.FolioCredito == folioCredito);
            if (expediente.Documentos.Count() > 0)
            {
                return Ok(expediente);
            }
            else
            {
                return NotFound("No hay mano");
            }
        }


        [HttpGet("listar-valijas-enviadas/{marcavance?}")]
        public IActionResult ListarValijas(string marcavance="")
        {
            var valijas = _solicitudRepository.listarValijasEnviadas(marcavance);

            return Ok(valijas);
        }


        [HttpGet("listar-valijas-enviadas-oficina/{marcavance?}")]
        public IActionResult ListarValijasOficina(string marcavance = "")
        {
            var codificacionOficinaLogedIn = User.Claims.FirstOrDefault(x => x.Type == CustomClaimTypes.OficinaCodigo).Value;
            var valijas = _context.ValijasOficinas
                                .Include(v => v.Expedientes)
                                .Include(v => v.OficinaDestino)
                                .Include(v => v.OficinaEnvio)
                                .Where(val => val.OficinaDestino.Codificacion == codificacionOficinaLogedIn)
                                .ToList();

            if (!string.IsNullOrEmpty(marcavance))
            {
                valijas = valijas.Where(d => d.MarcaAvance == marcavance).ToList();
            }

            return Ok(valijas);
        }


        [HttpGet("listar-cajas-enviadas/{marcavance?}")]
        public IActionResult ListarCajas(string marcavance = "")
        {
            var valijas = _context.CajasValoradas.Include(v => v.Expedientes).ToList();

            if (!string.IsNullOrEmpty(marcavance))
            {
                valijas = valijas.Where(d => d.MarcaAvance == marcavance).ToList();
                
            }
            generarexcel(valijas);//Rafa quitar solo prueba 
            return Ok(valijas);
        }

        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
        public IActionResult generarexcel(dynamic list)
        {
            DataTable data = new DataTable();
            data = ToDataTable(list);
            using (var libro = new XLWorkbook())
            {
                data.TableName = "Reporte_" + DateTime.Now.ToString("M");
                var hoja = libro.Worksheets.Add(data);
                hoja.ColumnsUsed().AdjustToContents();

                using (var memoria = new MemoryStream())
                {
                    libro.SaveAs(memoria);
                    //libro.SaveAs("C://Users/13771155-9/Desktop/as400/libro.xlsx");
                    var nombreExcel = "libro.xlsx";
                    return File(memoria.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreExcel);
                }
            }
        }

        [HttpGet("listar-expedientes-valija/{folioValija}")]
        public IActionResult ListarExpedientesValija(string folioValija)
        {
            var expedientes = _context.ExpedientesCreditos.Include(e => e.Documentos).Include(e => e.Credito).Where(d => d.ValijaValorada.CodigoSeguimiento == folioValija);
            return Ok(expedientes);
        }

        [HttpGet("listar-expedientes-caja/{folioCaja}")]
        public IActionResult ListarExpedientesCaja(string folioCaja)
        {
            var expedientes = _context.ExpedientesCreditos.Include(e => e.Documentos).Include(e => e.Credito).Where(d => d.CajaValorada.CodigoSeguimiento == folioCaja);
            return Ok(expedientes);
        }

        [HttpGet("listar-expedientes-oficina/{folioCaja}")]
        public IActionResult ListarExpedientesCajaOficina(string folioCaja)
        {
            var expedientes = _context.ExpedientesCreditos.Include(e => e.Documentos).Include(e => e.Credito).Where(d => d.ValijaOficina.CodigoSeguimiento == folioCaja);
            return Ok(expedientes);
        }
        
    }
}
