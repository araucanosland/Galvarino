using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Galvarino.Web.Data;
using Microsoft.EntityFrameworkCore;
using Galvarino.Web.Services.Workflow;
using Galvarino.Web.Models.Workflow;
using Microsoft.AspNetCore.Authorization;
using Rotativa.AspNetCore;
using Rotativa.AspNetCore.Options;
using Galvarino.Web.Models.Helper.Pensionado;
using Galvarino.Web.Models.Application.Pensionado;

namespace Galvarino.Web.Controllers.Pensionados
{
    [Route("/salidas/pensionado/pdf")]
    [Authorize]
    public class PdfController : Controller
    {
        private readonly PensionadoDbContext _context;


        public PdfController(PensionadoDbContext context)
        {
            _context = context;
        }




        [Route("detalle-valija-valorada/{codigoSeguimiento}")]
        public async Task<IActionResult> DetalleValijaValoradaPensionado([FromRoute] string codigoSeguimiento)
        {
            var variable = new List<Variable>();
            var valija = _context.ValijasValoradas.Include(h => h.HomologacionOficinas).Where(v => v.CodigoSeguimiento == codigoSeguimiento).FirstOrDefault();

            ICollection<Documentos> LDocumentos = new List<Documentos>();
            ICollection<Documentos> documentosExpediente = new List<Documentos>();
            ICollection<PdfParametros> expedientes = new List<PdfParametros>();
           
            var expPdf= from e in _context.Expedientes
                         join p in _context.Pensionado on e.Pensionado.Id equals p.Id
                         join ti in _context.Tipo on p.TipoPensionado equals ti.Id
                         where e.ValijaValoradaId == valija.Id
                         select new { e.Id, p.Folio ,p.RutCliente , ti.TipoDescripcion,ti.Motivo, e.FechaCreacion}
                        ;

            foreach (var exp in expPdf)
            {
                PdfParametros pdfParametros = new PdfParametros();
                documentosExpediente = _context.Documentos.Include(cd => cd.ConfiguracionDocumento).Where(e => e.ExpedienteId == exp.Id).ToArray();
                foreach (var docExp in documentosExpediente)
                {
                    LDocumentos.Add(docExp);
                }
                pdfParametros.Id= exp.Id;
                pdfParametros.Folio = exp.Folio;
                pdfParametros.TipoDescripcion = exp.TipoDescripcion;
                pdfParametros.Motivo = exp.Motivo;
                pdfParametros.FechaCreacion = exp.FechaCreacion;        
                pdfParametros.RutCliente = exp.RutCliente;
                expedientes.Add(pdfParametros);
            }




            return new ViewAsPdf(new PdfModelHelper()
            {
                FechaImpresion = valija.FechaEnvio.ToShortDateString(),
                CodigoSeguimiento = codigoSeguimiento,
                ValijaValorada = valija,
                Expedientes = expedientes,
                Documentos = LDocumentos
            })
            {
                PageSize = Size.Letter
            };
        }



    //    [Route("detalle-nomina-reparo/{codigoSeguimiento}")]
    //    public async Task<IActionResult> DetalleNominaRepoaroOficina(string codigoSeguimiento)
    //    {
    //        var variable = new List<Variable>();
    //        var valorada = await _context.ValijasValoradas
    //                            .Include(pm => pm.Expedientes)
    //                                .ThenInclude(e => e.Credito)
    //                            .Include(pm => pm.Expedientes)
    //                                .ThenInclude(e => e.Documentos)
    //                            .Include(pm => pm.Oficina)
    //                            .FirstOrDefaultAsync(pn => pn.CodigoSeguimiento == codigoSeguimiento);
    //        foreach (var credir in valorada.Expedientes)
    //        {
    //            var varibale = new Variable();
    //            varibale = _context.Variables.Where(p => p.NumeroTicket == credir.Credito.NumeroTicket && p.Clave == "DESCRIPCION_REPARO" && p.Tipo == "string").FirstOrDefault();
    //            if (varibale != null)
    //            {
    //                variable.Add(varibale);
    //            }
    //        }


    //        return new ViewAsPdf(new PdfModelHelper()
    //        {
    //            FechaImpresion = valorada.FechaEnvio.ToShortDateString(),
    //            CodigoSeguimiento = codigoSeguimiento,
    //            ValijaValorada = valorada,
    //            Variables = variable
    //        })
    //        {
    //            PageSize = Size.Letter
    //        };
    //    }

    //    [Route("test")]
    //    public IActionResult Index()
    //    {
            
    //        return new ViewAsPdf(new PdfModelHelper(){
    //            FechaImpresion = DateTime.Now.ToShortDateString()
    //        })
    //        {
    //            PageSize = Size.Letter
    //        };
    //    }


    //    [Route("detalle-pack-notaria/{codigoSeguimiento}")]
    //    public async Task<IActionResult> DetallePackNotaria(string codigoSeguimiento)
    //    {
    //        var pack = await _context.PacksNotarias
    //                            .Include(pm => pm.Expedientes)  
    //                                .ThenInclude(e => e.Credito)
    //                            .Include(pm => pm.Expedientes)
    //                                .ThenInclude(e => e.Documentos)    
    //                            .Include(pm => pm.NotariaEnvio)
    //                            .Include(pm => pm.Oficina)
    //                            .FirstOrDefaultAsync(pn => pn.CodigoSeguimiento == codigoSeguimiento);
            
    //        return new ViewAsPdf(new PdfModelHelper()
    //        {
    //            FechaImpresion = pack.FechaEnvio.ToShortDateString(),
    //            CodigoSeguimiento = codigoSeguimiento,
    //            PackNotaria = pack,
    //            MarcaDocto = codigoSeguimiento.Contains("R") ? "REPARO" : "OTRO"
    //        })
    //        {
    //            PageSize = Size.Letter
    //        };
    //    }

       

    //    [Route("detalle-caja-valorada/{codigoSeguimiento}")]
    //    public async Task<IActionResult> DetalleCajaValorada(string codigoSeguimiento)
    //    {
    //        var valorada = await _context.CajasValoradas
    //                            .Include(pm => pm.Expedientes)
    //                                .ThenInclude(e => e.Credito)
    //                            .Include(pm => pm.Expedientes)
    //                                .ThenInclude(e => e.Documentos)
    //                            .FirstOrDefaultAsync(pn => pn.CodigoSeguimiento == codigoSeguimiento);

    //        return new ViewAsPdf(new PdfModelHelper()
    //        {
    //            FechaImpresion = valorada.FechaEnvio.ToShortDateString(),
    //            CodigoSeguimiento = codigoSeguimiento,
    //            CajaValorada = valorada
    //        })
    //        {
    //            PageSize = Size.Letter
    //        };
    //    }

    //    [Route("detalle-valijas-oficinas/{codigoSeguimiento}")]
    //    public async Task<IActionResult> DetalleValijaValoradaOficina(string codigoSeguimiento)
    //    {


    //        var valorada = await _context.ValijasOficinas
    //                            .Include(pm => pm.Expedientes)
    //                                .ThenInclude(e => e.Credito)
    //                            .Include(pm => pm.Expedientes)
    //                                .ThenInclude(e => e.Documentos)
    //                            .Include(pm => pm.OficinaDestino)
    //                            .Include(pm => pm.OficinaEnvio)
    //                            .FirstOrDefaultAsync(pn => pn.CodigoSeguimiento == codigoSeguimiento);

    //        return new ViewAsPdf(new PdfModelHelper()
    //        {
    //            FechaImpresion = valorada.FechaEnvio.ToShortDateString(),
    //            CodigoSeguimiento = codigoSeguimiento,
    //            ValijaOficina = valorada
    //        })
    //        {
    //            PageSize = Size.Letter
    //        };
    //    }

    //    [Route("detalle-valijas-diario")]
    //    public async Task<IActionResult> DetalleValijasDiario()
    //    {
    //        var model = from valija in _context.ValijasValoradas
    //                        .Include(vl => vl.Oficina)
    //                        .Include(val => val.Expedientes)
    //                        .ThenInclude(exp => exp.Credito)
    //                    join tarea in _context.Tareas   
    //                    .Include(tr => tr.Solicitud)
    //                    .Include(tr => tr.Etapa) on valija.Expedientes.FirstOrDefault().Credito.NumeroTicket equals tarea.Solicitud.NumeroTicket
    //                    where tarea.Etapa.NombreInterno == ProcesoDocumentos.ETAPA_RECEPCION_VALIJA_OFICINA_PARTES
    //                    && tarea.Estado == EstadoTarea.Finalizada
    //                    && tarea.FechaTerminoFinal.Value.Date  ==  DateTime.Now.Date
    //                    select new ReporteValija{
    //                        folioValija = valija.CodigoSeguimiento,
    //                        oficina = valija.Oficina.Nombre,
    //                        cantidadExpedientes = valija.Expedientes.Count,
    //                        fechaPistoleo = tarea.FechaTerminoFinal
    //                    };
    //        var toshow = model.Distinct().OrderBy(x => x.fechaPistoleo);
    //        return new ViewAsPdf(toshow)
    //        {
    //            PageSize = Size.Letter
    //        };
    //    }

    //    [Route("detalle-caja-comercial/{codigoSeguimiento}")]
    //    public IActionResult DetalleCajaComercial([FromRoute] string codigoSeguimiento)
    //    {
    //        var model =  _context.AlmacenajesComerciales
    //                    .Include(valc => valc.Expedientes)
    //                    .ThenInclude(exp => exp.Credito)
    //                    .FirstOrDefault(df => df.CodigoSeguimiento == codigoSeguimiento);
            
    //        model.Expedientes = model.Expedientes.OrderBy(x => x.Credito.FechaDesembolso).ThenBy(x => x.Credito.FolioCredito).ToList();
    //        return new ViewAsPdf(model)
    //        {
    //            PageSize = Size.Letter
    //        };
    //    }
    }
}