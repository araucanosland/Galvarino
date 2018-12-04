using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Galvarino.Web.Models;
using Galvarino.Web.Data;
using Microsoft.EntityFrameworkCore;
using Galvarino.Web.Services.Workflow;
using Galvarino.Web.Models.Application;
using Galvarino.Web.Models.Workflow;
using Microsoft.AspNetCore.Authorization;
using Galvarino.Web.Services.Notification;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;
using Galvarino.Web.Models.Helper;

namespace Galvarino.Web.Controllers
{
    [Route("salidas/pdf")]
    [Authorize]
    public class PdfController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWorkflowService _wfService;

        public PdfController(ApplicationDbContext context, IWorkflowService wfservice)
        {
            _context = context;
            _wfService = wfservice;
        }

        [Route("test")]
        public IActionResult Index()
        {
            
            return new ViewAsPdf(new PdfModelHelper(){
                FechaImpresion = DateTime.Now.ToShortDateString()
            })
            {
                PageSize = Size.Letter
            };
        }


        [Route("detalle-pack-notaria/{codigoSeguimiento}")]
        public async Task<IActionResult> DetallePackNotaria(string codigoSeguimiento)
        {
            var pack = await _context.PacksNotarias
                                .Include(pm => pm.Expedientes)  
                                    .ThenInclude(e => e.Credito)
                                .Include(pm => pm.Expedientes)
                                    .ThenInclude(e => e.Documentos)    
                                .Include(pm => pm.NotariaEnvio)
                                .Include(pm => pm.Oficina)
                                .FirstOrDefaultAsync(pn => pn.CodigoSeguimiento == codigoSeguimiento);
            
            return new ViewAsPdf(new PdfModelHelper()
            {
                FechaImpresion = DateTime.Now.ToShortDateString(),
                CodigoSeguimiento = codigoSeguimiento,
                PackNotaria = pack,
                MarcaDocto = codigoSeguimiento.Contains("R") ? "REPARO" : "OTRO"
            })
            {
                PageSize = Size.Letter
            };
        }

        [Route("detalle-valija-valorada/{codigoSeguimiento}")]
        public async Task<IActionResult> DetalleValijaValorada([FromRoute] string codigoSeguimiento)
        {
            var valorada = await _context.ValijasValoradas
                                .Include(pm => pm.Expedientes)
                                    .ThenInclude(e => e.Credito)
                                .Include(pm => pm.Expedientes)
                                    .ThenInclude(e => e.Documentos)
                                .Include(pm => pm.Oficina)
                                .FirstOrDefaultAsync(pn => pn.CodigoSeguimiento == codigoSeguimiento);
            return new ViewAsPdf(new PdfModelHelper()
            {
                FechaImpresion = DateTime.Now.ToShortDateString(),
                CodigoSeguimiento = codigoSeguimiento,
                ValijaValorada = valorada
            })
            {
                PageSize = Size.Letter
            };
        }

        [Route("detalle-caja-valorada/{codigoSeguimiento}")]
        public async Task<IActionResult> DetalleCajaValorada(string codigoSeguimiento)
        {
            var valorada = await _context.CajasValoradas
                                .Include(pm => pm.Expedientes)
                                    .ThenInclude(e => e.Credito)
                                .Include(pm => pm.Expedientes)
                                    .ThenInclude(e => e.Documentos)
                                .FirstOrDefaultAsync(pn => pn.CodigoSeguimiento == codigoSeguimiento);

            return new ViewAsPdf(new PdfModelHelper()
            {
                FechaImpresion = DateTime.Now.ToShortDateString(),
                CodigoSeguimiento = codigoSeguimiento,
                CajaValorada = valorada
            })
            {
                PageSize = Size.Letter
            };
        }

        [Route("detalle-valijas-oficinas/{codigoSeguimiento}")]
        public async Task<IActionResult> DetalleValijaValoradaOficina(string codigoSeguimiento)
        {


            var valorada = await _context.ValijasOficinas
                                .Include(pm => pm.Expedientes)
                                    .ThenInclude(e => e.Credito)
                                .Include(pm => pm.Expedientes)
                                    .ThenInclude(e => e.Documentos)
                                .Include(pm => pm.OficinaDestino)
                                .Include(pm => pm.OficinaEnvio)
                                .FirstOrDefaultAsync(pn => pn.CodigoSeguimiento == codigoSeguimiento);

            return new ViewAsPdf(new PdfModelHelper()
            {
                FechaImpresion = DateTime.Now.ToShortDateString(),
                CodigoSeguimiento = codigoSeguimiento,
                ValijaOficina = valorada
            })
            {
                PageSize = Size.Letter
            };
        }

        [Route("detalle-valijas-diario")]
        public async Task<IActionResult> DetalleValijasDiario()
        {
            var model = from valija in _context.ValijasValoradas
                            .Include(vl => vl.Oficina)
                            .Include(val => val.Expedientes)
                            .ThenInclude(exp => exp.Credito)
                        join tarea in _context.Tareas   
                        .Include(tr => tr.Solicitud)
                        .Include(tr => tr.Etapa) on valija.Expedientes.FirstOrDefault().Credito.NumeroTicket equals tarea.Solicitud.NumeroTicket
                        where tarea.Etapa.NombreInterno == ProcesoDocumentos.ETAPA_RECEPCION_VALIJA_OFICINA_PARTES
                        && tarea.Estado == EstadoTarea.Finalizada
                        && tarea.FechaTerminoFinal.Value.Date  ==  DateTime.Now.Date
                        select new ReporteValija{
                            folioValija = valija.CodigoSeguimiento,
                            oficina = valija.Oficina.Nombre,
                            cantidadExpedientes = valija.Expedientes.Count,
                            fechaPistoleo = tarea.FechaTerminoFinal
                        };
            var toshow = model.Distinct();
            return new ViewAsPdf(toshow)
            {
                PageSize = Size.Letter
            };
        }

        [Route("detalle-caja-comercial/{codigoSeguimiento}")]
        public IActionResult DetalleCajaComercial([FromRoute] string codigoSeguimiento)
        {
            var model =  _context.AlmacenajesComerciales
                        .Include(valc => valc.Expedientes)
                        .ThenInclude(exp => exp.Credito)
                        .FirstOrDefault(df => df.CodigoSeguimiento == codigoSeguimiento);
                            
            return new ViewAsPdf(model)
            {
                PageSize = Size.Letter
            };
        }
    }
}