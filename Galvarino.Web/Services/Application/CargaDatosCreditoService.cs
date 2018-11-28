using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Galvarino.Web.Data;
using Galvarino.Web.Models.Application;
using Galvarino.Web.Services.Workflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Galvarino.Web.Models.Helper;
using Microsoft.Extensions.Configuration;

namespace Galvarino.Web.Services.Application
{
    public class CargaDatosCreditoService : BackgroundService
    {
        private readonly ILogger<CargaDatosCreditoService> _logger;
        private readonly IServiceScope _scope;
        private IWorkflowService _wfservice;
        private readonly IConfiguration _configuration;

        public CargaDatosCreditoService(ILogger<CargaDatosCreditoService> logger, IServiceProvider services, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _scope = services.CreateScope();
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug($"tarea en segundo plano esta iniciando");
            stoppingToken.Register(() => _logger.LogDebug("Deteniendo la tarea en segundo plano"));
            var _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("Ejecutando tarea en segundo plano");

                /*Todo:  Revisar Findesemanas*/
                string nombreArchivo = "Carga" + DateTime.Now.AddDays(-1).ToString("ddMMyyyy");
                string ruta = _configuration["RutaCargaCredito"] + nombreArchivo + ".txt";
                _logger.LogDebug(ruta);

                var existencia = _context.CargasIniciales.Where(x => x.NombreArchivoCarga == nombreArchivo).ToList();
                if ((File.Exists(ruta) && existencia.Count == 0) || (File.Exists(ruta) && File.ReadLines(ruta).Count()-1 > existencia.Count))
                {
                    _logger.LogDebug("Cargando Archivo a la BD (Carga Inicial diaria)......");
                    
                    int lap = 0;
                    int emperzardenuevo = File.ReadLines(ruta).Count() - 1 > existencia.Count ? existencia.Count+1 : 1;
                    foreach (var linea in File.ReadLines(ruta))
                    {
                        if (lap > 0 && emperzardenuevo == lap)
                        {
                            emperzardenuevo++;
                            string[] campos = linea.Split(new char[] { ';' });
                            CargaInicial ci = new CargaInicial
                            {
                                RutAfiliado = campos[0],
                                FolioCredito = campos[1],
                                CodigoOficinaIngreso = campos[2],
                                CodigoOficinaPago = campos[4],
                                Estado = campos[6],
                                LineaCredito = campos[7],
                                RutResponsable = campos[8],
                                CanalVenta = campos[9],
                                FechaVigencia = campos[10],
                                FechaCarga = DateTime.Now,
                                FechaCorresponde = DateTime.Now.AddDays(-1),
                                NombreArchivoCarga = nombreArchivo
                            };
                            _context.CargasIniciales.Add(ci);

                            
                            var oficinaProceso = _context.Oficinas.Include(x => x.OficinaProceso).FirstOrDefault(x => x.Codificacion == ci.CodigoOficinaPago);
                            string esRM = oficinaProceso.EsRM  ? "1" : "0";
                            

                            Dictionary<string, string> _setVariables = new Dictionary<string, string>();
                            _setVariables.Add("OFICINA_PAGO", campos[4]);
                            _setVariables.Add("OFICINA_INGRESO", campos[2]);
                            _setVariables.Add("FOLIO_CREDITO", campos[1]);
                            _setVariables.Add("RUT_AFILIADO", campos[0]);
                            _setVariables.Add("FECHA_VENTA", campos[10]);
                            _setVariables.Add("ES_RM", esRM);
                            _setVariables.Add("DOCUMENTO_LEGALIZADO", "0");
                            _setVariables.Add("OFICINA_PROCESA_NOTARIA", oficinaProceso.OficinaProceso.Codificacion);


                            
                            _wfservice = new WorkflowService(new DefaultWorkflowKernel(_context));
                            var wf = _wfservice.Instanciar(ProcesoDocumentos.NOMBRE_PROCESO, "wfboot", "Ingreso Automatico de Creditos Vendidos", _setVariables);
                            
                            Credito cred = new Credito
                            {
                                FechaDesembolso = DateTime.Now.AddDays(-1),
                                FechaFormaliza = DateTime.Now.AddDays(-1),
                                FolioCredito = ci.FolioCredito,
                                MontoCredito = 0,
                                RutCliente = ci.RutAfiliado,
                                NumeroTicket = wf.NumeroTicket
                            };

                            if(ci.LineaCredito.ToLower().Contains("credito normal") && ci.Estado.Contains("Reprogramado"))
                            {
                                cred.TipoCredito = TipoCredito.Reprogramacion;
                            }
                            else if (ci.LineaCredito.ToLower().Contains("credito normal") || ci.LineaCredito.ToLower().Contains("compra cartera") || ci.LineaCredito.ToLower().Contains("credito paralelo"))
                            {
                                cred.TipoCredito = TipoCredito.Normal;
                            }
                            else if (ci.LineaCredito.ToLower().Contains("reprogr"))
                            {
                                cred.TipoCredito = TipoCredito.Reprogramacion;
                            }
                            else if(ci.LineaCredito.ToLower().Contains("acuerdo de creditos castigados"))
                            {
                                cred.TipoCredito = TipoCredito.AcuerdoPago;
                            }

                            

                            IEnumerable<ConfiguracionDocumento> configs = _context.ConfiguracionDocumentos.Where(x => x.TipoCredito == cred.TipoCredito && x.TipoExpediente == TipoExpediente.Legal).ToList();


                            ExpedienteCredito expcred = new ExpedienteCredito
                            {
                                Credito = cred,
                                FechaCreacion = DateTime.Now,
                                TipoExpediente = TipoExpediente.Legal,
                            };

                            int incrementor = 1;
                            foreach (var confItem in configs)
                            {
                                Documento docmnt = new Documento{
                                    TipoDocumento = confItem.TipoDocumento,
                                    Codificacion = confItem.Codificacion,
                                    Resumen = confItem.TipoDocumento.ToString("D")
                                };
                                expcred.Documentos.Add(docmnt);
                                incrementor++;
                            }
                            _context.ExpedientesCreditos.Add(expcred);
                            await _context.SaveChangesAsync();
                        }
                        lap++;
                    }
                    _logger.LogDebug("Carga terminada");
                }
                else
                {
                    _logger.LogDebug("Nada que cargar");
                }
                
                _logger.LogDebug("Esperando!!!!");    
                await Task.Delay(60000, stoppingToken);
            }
        }
    }
}
