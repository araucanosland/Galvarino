using Dapper;
using Galvarino.Web.Data;
using Galvarino.Web.Models.Application.Pensionado;
using Galvarino.Web.Models.Mappings;
using Galvarino.Web.Services.Notification;
using Galvarino.Web.Services.Workflow;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TinyCsvParser;

namespace Galvarino.Web.Workers
{
    internal class CargaInicialPensionado : BackgroundService
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

        public CargaInicialPensionado(ILogger<CargaInicialPensionado> logger, IServiceProvider services, IConfiguration configuration, INotificationKernel mailService)
        {
            _logger = logger;
            _configuration = configuration;
            _mailService = mailService;
            _scope = services.CreateScope();

            this.setHora();
        }

        public CargaInicialPensionado()
        {

        }


        public override void Dispose() => _timer?.Dispose();

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private  void DoWork(object state)
        {
            var _context= _scope.ServiceProvider.GetRequiredService<PensionadoDbContext>();
           
            string rutaDescargar = _configuration.GetValue<string>("RutaArchivoPensionado");// @"C:\Desarrollos\Archivos\Galvarino\";
            string nombreArchivo = _configuration.GetValue<string>("ArchivoOportunidadPensionado");// "Aux_OportunidadesPensionados.txt";

            string Schema = _configuration.GetValue<string>("schemaPensionado");

            #region carga_oportunidad
            
            StringBuilder inserts = new StringBuilder();
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DocumentManagementConnection")))
            {
                string sql = "";
                int existeCarga;
                string SchemaEstado = _configuration.GetValue<string>("schema");
                sql = "SELECT * " +
                      " FROM " + SchemaEstado + ".CargasInicialesEstado CIE  " +
                      "WHERE CIE.NombreArchivoCarga = '" + nombreArchivo + "' " +
                      "  AND CONVERT(varchar,CIE.FechaCarga,120) BETWEEN '" + DateTime.Today.ToString("yyyy-MM-dd") + " 00:00:00.000'" +
                      "                                              AND '" + DateTime.Today.ToString("yyyy-MM-dd") + " 23:59:59.999';";

                existeCarga = 1;// await connection.QueryFirstAsync<int>(sql);

                DateTime elDia = Convert.ToDateTime(DateTime.Today.ToString("yyyy-MM-dd"));

                var valida = _context.CargasInicialesEstado.Where(p => p.NombreArchivoCarga == nombreArchivo && p.FechaCarga.Date >= elDia).FirstOrDefault();
                

                var cargasIniciales = _context.CargasIniciales.Include(p => p.Sucursal).Include(t => t.Tipo).FirstOrDefault();


                DateTime elDia2 = Convert.ToDateTime(DateTime.Today.ToString("yyyy-MM-dd"));
                CargasInicialesEstado ci = new CargasInicialesEstado();
                ci.NombreArchivoCarga = "adsdasdasd";
                ci.Estado = "asdassad";
                ci.FechaCarga = elDia2;


                _context.CargasInicialesEstado.Add(ci);
                _context.SaveChanges();



                if (valida!=null)
                {
                    CsvParserOptions csvParserOptions = new CsvParserOptions(true, ';');
                    CargaInicialPensionadoMapping csvMapper = new CargaInicialPensionadoMapping();
                    CsvParser<CargaInicialPensionadoIM> csvParser = new CsvParser<CargaInicialPensionadoIM>(csvParserOptions, csvMapper);

                    var result = csvParser
                        .ReadFromFile(rutaDescargar + nombreArchivo, Encoding.ASCII)
                        .Where(x => x.IsValid)
                        .Select(x => x.Result)
                        .AsSequential()
                        .ToList();
               
                    result.ForEach(x => inserts.AppendLine($"insert into {Schema}.Cargasiniciales " +
                                                               $"(FechaCarga," +
                                                               $"FechaProceso," +
                                                               $"Folio,Estado," +
                                                               $"RutPensionado," +
                                                               $"DvPensionado," +
                                                               $"NombrePensionado," +
                                                               $"TipoID,FechaSolicitud," +
                                                               $"FechaEfectiva," +
                                                               $"SucursalId," +
                                                               $"Forma," +
                                                               $"TipoMovimiento) " +
                                                           $"values (" +
                                                               $"'{DateTime.Now}'," +
                                                               $"'{x.FechaProceso}'," +
                                                               $"'{x.Folio}'," +
                                                               $"'{x.Estado}'," +
                                                               $"'{x.RutPensionado}'," +
                                                               $"'{x.DvPensionado}'," +
                                                               $"'{x.NombrePensionado + " " + x.Nombre2Pensionado + " " + x.ApellidoPatPensionado + " " + x.ApellidoMatPensionado}'," +
                                                               $"'{x.IdTipo}'," +
                                                               $"'{x.FechaSolicitud}'," +
                                                               $"'{x.FechaEfectiva}'," +
                                                               $"'{x.IdSucursal.Replace("O ", "")}'," +
                                                               $"'{x.Tipo}'," +
                                                               $"'AFILIACION');"));

                    connection.Execute(inserts.ToString(), null, null, 240);
                 

                    string insertarCargasInincialesEstado = @"INSERT INTO " + SchemaEstado + ".CargasInicialesEstado" +
                                                                "(fechacarga,NombreArchivoCarga,Estado) " +
                                                            "VALUES (getdate(),'" + nombreArchivo + "','PencionadosOportunidadParcial')";
                    connection.Execute(insertarCargasInincialesEstado.ToString(), null, null, 240);
                }
                else
                {
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
