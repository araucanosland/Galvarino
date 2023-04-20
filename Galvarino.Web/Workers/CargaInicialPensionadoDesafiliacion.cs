using Dapper;
using Galvarino.Web.Models.Mappings;
using Galvarino.Web.Services.Notification;
using Galvarino.Web.Services.Workflow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TinyCsvParser;

namespace Galvarino.Web.Workers
{

    internal class CargaInicialPensionadoDesafiliacion : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly INotificationKernel _mailService;
        private IWorkflowService _wfservice;
        private readonly IServiceScope _scope;
        private Timer _timer;
        private bool estaOcupado = false;
        private TimeSpan horaInicial;
        private TimeSpan horaFinal;


        public CargaInicialPensionadoDesafiliacion(ILogger<CargaInicialPensionado> logger, IServiceProvider services, IConfiguration configuration, INotificationKernel mailService)
        {
            _logger = logger;
            _configuration = configuration;
            _mailService = mailService;
            _scope = services.CreateScope();

            this.setHora();
        }

        public CargaInicialPensionadoDesafiliacion()
        {

        }


        public override void Dispose() => _timer?.Dispose();

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {

            string rutaDescargar = _configuration.GetValue<string>("RutaArchivoPensionado");// @"C:\Desarrollos\Archivos\Galvarino\";
            string nombreArchivo = _configuration.GetValue<string>("ArchivoDesafilliacionPensionado");// "DESAFILIACION_PENSIONADO.csv";


            string Schema = _configuration.GetValue<string>("schemaPensionado");

            #region carga_desafiliacion
            

            Common.Utils utils = new Common.Utils();

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DocumentManagementConnection")))
            {
                string sql = "";
                int existeCarga;
                string SchemaEstado = _configuration.GetValue<string>("schema");

                sql = "SELECT COUNT(1) "+
                      " FROM " + SchemaEstado + ".CargasInicialesEstado CIE  " +
                      "WHERE CIE.NombreArchivoCarga = '"+ nombreArchivo + "' " +
                      "  AND CONVERT(varchar,CIE.FechaCarga,120) BETWEEN '" + DateTime.Today.ToString("yyyy-MM-dd") + " 00:00:00.000'" +
                      "                                              AND '" + DateTime.Today.ToString("yyyy-MM-dd") + " 23:59:59.999';";

                existeCarga = connection.Query<int>(sql).FirstOrDefault();

                if (existeCarga == 0)
                {
                    CsvParserOptions csvParserOptions = new CsvParserOptions(true, ';');
                    CargaInicialPensionadoDesafiliacionMapping csvMapper = new CargaInicialPensionadoDesafiliacionMapping();
                    CsvParser<CargaInicialPensionadoDesafiliacionIM> csvParser = new CsvParser<CargaInicialPensionadoDesafiliacionIM>(csvParserOptions, csvMapper);

                    var archivo = rutaDescargar + nombreArchivo;

                    var result = csvParser
                        .ReadFromFile(archivo, Encoding.ASCII)
                        .Where(x => x.IsValid)
                        .Select(x => x.Result)
                        .AsSequential()
                        .ToList();

                    StringBuilder inserts = new StringBuilder();
                    result.ForEach(x => inserts.AppendLine($"INSERT INTO {Schema}.Cargasiniciales " +
                                                               $"(FechaCarga, " +
                                                               $"FechaProceso, " +
                                                               $"Folio, " +
                                                               $"Estado, " +
                                                               $"RutPensionado, " +
                                                               $"DvPensionado, " +
                                                               $"NombrePensionado, " +
                                                               $"TipoId, " +
                                                               $"FechaSolicitud, " +
                                                               $"FechaEfectiva, " +
                                                               $"SucursalId, " +
                                                               $"Forma, " +
                                                               $"TipoMovimiento) " +
                                                           $"VALUES(" +
                                                               $"'{DateTime.Now}', " +
                                                               $"'{x.FechaProceso}', " +
                                                               $"N'{x.Folio}', " +
                                                               $"N'{x.Estado}', " +
                                                               $"N'{x.RutPensionado}', " +
                                                               $"N'{x.DvPensionado}', " +
                                                               $"N'{x.NombrePensionado}', " +
                                                               $"N'{x.IdTipo}', " +
                                                               $"'{x.FechaSolicitud}', " +
                                                               $"'{x.FechaEfectiva}', " +
                                                               $"'{x.IdSucursal.Replace("O ", "")}', " +
                                                               $"N'{utils.QuitaAcento(x.Tipo)}', " +
                                                               $"N'DESAFILIACION');"
                                                               ));

                    connection.Execute(inserts.ToString(), null, null, 240);

                    string insertarCargasInincialesEstado = @"INSERT INTO " + SchemaEstado + ".CargasInicialesEstado" +
                                                                "(fechacarga,NombreArchivoCarga,Estado) " +
                                                            "VALUES (getdate(),'" + nombreArchivo + "','PencionadosDesafiliacionParcial')";
                    connection.Execute(insertarCargasInincialesEstado.ToString(), null, null, 240);

                }
                else {
                    Debug.WriteLine("Ya existe carga de [" + nombreArchivo + "] con fecha [" + DateTime.Today.ToString("yyyy-MM-dd") + "]");
                }
            }
            #endregion
        }


        private void setHora()
        {
            var rawMomentoInicio = _configuration.GetValue<string>("CoordinacionWorkers:CargaInicialCreditosWorker:HoraInicio");
            int hInicio = Convert.ToInt32(rawMomentoInicio.Split(':')[0]);
            int mInicio = Convert.ToInt32(rawMomentoInicio.Split(':')[1]);
            int sInicio = Convert.ToInt32(rawMomentoInicio.Split(':')[2]);

            this.horaInicial = new TimeSpan(hInicio, mInicio, sInicio);

            var rawMomentoFin = _configuration.GetValue<string>("CoordinacionWorkers:CargaInicialCreditosWorker:HoraFin");
            int hFin = Convert.ToInt32(rawMomentoFin.Split(':')[0]);
            int mFin = Convert.ToInt32(rawMomentoFin.Split(':')[1]);
            int sFin = Convert.ToInt32(rawMomentoFin.Split(':')[2]);

            this.horaFinal = new TimeSpan(hFin, mFin, sFin);

        }


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {

            object state = null;
            _timer = new Timer(DoWork, state, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }
    }
}
   
