using Microsoft.Extensions.Hosting;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;
using Galvarino.Web.Services.Workflow;
using Microsoft.Extensions.Configuration;
using Galvarino.Web.Models.Helper;
using Microsoft.Extensions.Logging;

namespace Galvarino.Web.Workers
{
    public enum TipoCredito
    {
        Normal,
        Reprogramacion,
        AcuerdoPago
    }
    /// <summary>
    /// Worker para corregir los documentos que queden estancados en workflow
    /// </summary>
    public class CorrectorCargaInicialWorker : BackgroundService
    {
        private Timer _theTimer;
        private bool itsBusy = false;
        private IWorkflowService _wfservice;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public CorrectorCargaInicialWorker(ILogger<CorrectorCargaInicialWorker> logger, IConfiguration configuration, IWorkflowService workflowService)
        {
            _configuration = configuration;
            _wfservice = workflowService;
            _logger = logger;
        }


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Worker Corrector Iniciando...");
            object state = null;
            _theTimer = new Timer(DoWork, state, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            return Task.CompletedTask;
        }


        public override Task StopAsync(CancellationToken cancellation)
        {
            Console.WriteLine("Worker Corrector Cerrando");
            _theTimer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _theTimer?.Dispose();
        }

        /// <summary>
        /// Trabajo del timer el cual se encarga de toda la logica
        /// </summary>
        /// <param name="state"></param>
        private void DoWork(object state)
        {
            if(!itsBusy)
            {
                itsBusy = true;
                using (var conn = new SqlConnection(_configuration.GetConnectionString("DocumentManagementConnection")))
                {
                    try
                    {
                        conn.Open();
                        var corregir = conn.Query<dynamic>(
                            @"SELECT *
                                FROM dbo.CargasIniciales
                                WHERE FolioCredito not in (
                                    SELECT FolioCredito FROM dbo.Creditos
                                )");
                        
                        List<dynamic> eDocumentos = new List<dynamic>();
                        
                        eDocumentos.Add(new
                        {
                            Resumen = 0,
                            Codificacion = $"01",
                            TipoDocumento = "Pagare"
                        });

                        eDocumentos.Add(new
                        {
                            Resumen = 1,
                            Codificacion = $"02",
                            TipoDocumento = "FotocopiaCedulaIdentidad"
                        });

                        eDocumentos.Add(new
                        {
                            Resumen = 4,
                            Codificacion = $"06",
                            TipoDocumento = "HojaProlongacion"
                        });
                        
                        eDocumentos.Add(new
                        {
                            Resumen = 5,
                            Codificacion = $"07",
                            TipoDocumento = "AcuerdoDePago"
                        });

                        foreach (var item in corregir)
                        {
                            /*
                             * TODO: 
                             *      - limpieza de variables WF
                             *      - Limpieza de Solicitud y tareas malas en WF
                             *      - Carga nueva en WF
                            */

                            try
                            {
                                /*Obtenemos el numero de ticket para limpieza profunda*/
                                var numeroTicket = conn.QueryFirst<string>(
                                                        @"SELECT NumeroTicket
                                                        FROM dbo.Variables 
                                                        WHERE Valor = '@FolioCredito'", new { item.FolioCredito });

                                /*Limpieza de WF variables y solicitudes mal-formadas*/
                                string cleanSQL = @"
                                DELETE FROM dbo.Variables
                                    WHERE NumeroTicket = '@NumeroTicket';

                                DELETE FROM dbo.Solicitudes
                                    WHERE NumeroTicket = '@NumeroTicket';
                                ";
                                //Aqui ya murio el antiguo numero de ticket

                                conn.Execute(cleanSQL, new { NumeroTicket = numeroTicket });
                            }
                            catch(Exception)
                            {
                                Console.WriteLine("Nada que borrar...");
                            }
                            finally
                            {
                                Console.WriteLine("Ya muerió el antguo ticket...");
                            }
                            

                            

                            var oficinaVenta = conn.QueryFirst<dynamic>(
                                @"SELECT a.Codificacion OficinaVenta, a.EsRM, a.EsMovil, b.Codificacion OficinaNodriza
                                    FROM dbo.Oficinas a
                                    INNER JOIN dbo.Oficinas b on a.OficinaProcesoId = b.Id
                                    WHERE a.Codificacion = @OficinaVentaCodigo", new { OficinaVentaCodigo = item.CodigoOficinaPago });
                            
                            
                            Dictionary<string, string> _setVariables = new Dictionary<string, string>();
                            _setVariables.Add("OFICINA_PAGO", item.CodigoOficinaPago);
                            _setVariables.Add("OFICINA_INGRESO", item.CodigoOficinaIngreso);
                            _setVariables.Add("FOLIO_CREDITO", item.FolioCredito);
                            _setVariables.Add("RUT_AFILIADO", item.RutAfiliado);
                            _setVariables.Add("FECHA_VENTA", item.FechaVigencia);
                            _setVariables.Add("ES_RM", oficinaVenta.EsRM ? @"1": @"0");
                            _setVariables.Add("DOCUMENTO_LEGALIZADO", @"0");
                            _setVariables.Add("OFICINA_PROCESA_NOTARIA", oficinaVenta.OficinaNodriza);


                            var wfInstance = _wfservice.Instanciar(ProcesoDocumentos.NOMBRE_PROCESO, "wfboot", "[REINGRESO] Ingreso Automatico de Creditos Vendidos", _setVariables);
                            Console.WriteLine("Workflow Instanciado...");

                            int elTipoCredito = 0;
                            if (item.LineaCredito.ToLower().Contains("credito normal") && item.Estado.Contains("Reprogramado"))
                            {
                                elTipoCredito = (int)TipoCredito.Reprogramacion;
                            }
                            else if (item.LineaCredito.ToLower().Contains("credito normal") || item.LineaCredito.ToLower().Contains("compra cartera") || item.LineaCredito.ToLower().Contains("credito paralelo"))
                            {
                                elTipoCredito = (int)TipoCredito.Normal;
                            }
                            else if (item.LineaCredito.ToLower().Contains("reprogr"))
                            {
                                elTipoCredito = (int)TipoCredito.Reprogramacion;
                            }
                            else if (item.LineaCredito.ToLower().Contains("acuerdo de creditos castigados"))
                            {
                                elTipoCredito = (int)TipoCredito.AcuerdoPago;
                            }

                            var cred = new
                            {
                                FechaDesembolso = item.FechaCorresponde,
                                FechaFormaliza = item.FechaCarga,
                                FolioCredito = item.FolioCredito,
                                MontoCredito = 0,
                                RutCliente = item.RutAfiliado,
                                NumeroTicket = wfInstance.NumeroTicket,
                                TipoCredito = elTipoCredito
                            };
                            
                            
                            var creditoSQL = @"
                            INSERT INTO dbo.Creditos (
                                                        FolioCredito,
                                                        MontoCredito,
                                                        FechaFormaliza,
                                                        FechaDesembolso,
                                                        RutCliente,
                                                        NumeroTicket,
                                                        TipoCredito
                                                     )
                            VALUES  (
                                        @FolioCredito,
                                        @MontoCredito,
                                        @FechaFormaliza,
                                        @FechaDesembolso,
                                        @RutCliente,
                                        @NumeroTicket,
                                        @TipoCredito
                                    );

                            SELECT SCOPE_IDENTITY()";
                            int CreditoId = conn.QueryFirst<int>(creditoSQL, cred);
                             
                            string expedienteSQL = @"
                                INSERT INTO dbo.ExpedientesCreditos 
                                (
                                    FechaCreacion,
                                    CreditoId,
                                    TipoExpediente
                                )
                                VALUES 
                                (
                                    @FechaCreacion,
                                    @CreditoId,
                                    @TipoExpediente
                                );
                                SELECT SCOPE_IDENTITY();";
                            int ExpedienteCreditoId = conn.QueryFirst<int>(expedienteSQL, new { FechaCreacion = DateTime.Now, CreditoId, TipoExpediente  = 0});
                            
                            var configs = conn.Query<dynamic>(@"SELECT * 
                                                FROM dbo.ConfiguracionDocumentos
                                                WHERE TipoCredito = @elTipoCredito
                                                AND TipoExpediente = 0", new { elTipoCredito });

                            string DocumentosSQL = "";
                            foreach (var confItem in configs)
                            {
                                var plt = eDocumentos.FirstOrDefault(x => x.Resumen == confItem.TipoDocumento);
                                DocumentosSQL += $@"INSERT INTO dbo.Documentos (
                                    Resumen,
                                    Codificacion,
                                    TipoDocumento,
                                    ExpedienteCreditoId

                                ) VALUES ('{plt?.Resumen}', '{plt?.Codificacion }', '{plt?.TipoDocumento}', {ExpedienteCreditoId});";
                            }
                            conn.Execute(DocumentosSQL);

                        }
                    }
                    catch (SqlException exception)
                    {
                        _logger.LogCritical($"FATAL ERROR: Database connections could not be opened: {exception.Message}");
                    }
                }

               itsBusy = false;
            }
        }


    }

    
}
