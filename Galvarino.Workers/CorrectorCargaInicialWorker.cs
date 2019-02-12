using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;

namespace Galvarino.Workers
{
    /// <summary>
    /// Worker para corregir los documentos que queden estancados en workflow
    /// </summary>
    public class CorrectorCargaInicialWorker : BackgroundService
    {
        private Timer _theTimer;
        private string _connectionString = "server=(LocalDb)\\MSSQLLocalDB;database=galvarino_db;uid=galvarino_db;password=secreto";
        private bool itsBusy = false;


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Worker Corrector Iniciando...");
            object state = null;
            _theTimer = new Timer(DoWork, state, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            return Task.CompletedTask;
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
                using (var conn = new SqlConnection(_connectionString))
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


                        foreach (var item in corregir)
                        {
                            /*
                             * TODO: 
                             *      - limpieza de variables WF
                             *      - Limpieza de Solicitud y tareas malas en WF
                             *      - Carga nueva en WF
                            */

                            /*Obtenemos el numero de ticket para limpieza profunda*/
                            var numeroTicket = conn.QueryFirst<string>(
                                                    @"SELECT NumeroTicket
                                                        FROM Variables 
                                                        WHERE Valor = '@FolioCredito'", new { item.FolioCredito});

                            /*Limpieza de WF variables y solicitudes mal-formadas*/
                            string cleanSQL = @"
                                DELETE FROM dbo.Variables
                                    WHERE NumeroTicket = '@NumeroTicket';

                                DELETE FROM dbo.Solicitudes
                                    WHERE NumeroTicket = '@NumeroTicket';
                            ";
                            
                            conn.Execute(cleanSQL, new { NumeroTicket = numeroTicket });
                            
                            var oficinaVenta = conn.QueryFirst<dynamic>(
                                @"SELECT a.Codificacion OficinaVenta, a.EsRM, a.EsMovil, b.Codificacion OficinaNodriza
                                    FROM dbo.Oficinas a
                                    INNER JOIN dbo.Oficinas b on a.OficinaProcesoId = b.Id
                                    WHERE a.Codificacion = '@OficinaVentaCodigo'", new { OficinaVentaCodigo = item.CodigoOficinaPago });
                            
                            
                            Dictionary<string, string> _setVariables = new Dictionary<string, string>();
                            _setVariables.Add("OFICINA_PAGO", item.CodigoOficinaPago);
                            _setVariables.Add("OFICINA_INGRESO", item.CodigoOficinaIngreso);
                            _setVariables.Add("FOLIO_CREDITO", item.FolioCredito);
                            _setVariables.Add("RUT_AFILIADO", item.RutAfiliado);
                            _setVariables.Add("FECHA_VENTA", item.FechaVigencia);
                            _setVariables.Add("ES_RM", oficinaVenta.EsRM);
                            _setVariables.Add("DOCUMENTO_LEGALIZADO", "0");
                            _setVariables.Add("OFICINA_PROCESA_NOTARIA", oficinaVenta.OficinaNodriza);



                        }
                    }
                    catch (SqlException exception)
                    {
                        Console.WriteLine($"Error en worker: {exception.Message}");
                        //_logger.LogCritical($"FATAL ERROR: Database connections could not be opened: {exception.Message}");
                    }
                }

               itsBusy = false;
            }
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
    }
}
