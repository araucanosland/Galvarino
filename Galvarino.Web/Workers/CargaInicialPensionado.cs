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

        private void DoWork(object state)
        {
           
            string rutaDescargar = @"C:\Desarrollos\Archivos\Galvarino\";
            string nombreArchivo = "Aux_OportunidadesPensionados.txt";

            string Schema = _configuration.GetValue<string>("schemaPensionado");
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DocumentManagementConnection")))
            {
                CsvParserOptions csvParserOptions = new CsvParserOptions(true, ';');
                CargaInicialPensionadoMapping csvMapper = new CargaInicialPensionadoMapping();
                CsvParser<CargaInicialPensionadoIM> csvParser = new CsvParser<CargaInicialPensionadoIM>(csvParserOptions, csvMapper);

                var result = csvParser
                    .ReadFromFile(rutaDescargar+nombreArchivo, Encoding.ASCII)
                    .Where(x => x.IsValid)
                    .Select(x => x.Result)
                    .AsSequential()
                    .ToList();
                StringBuilder inserts = new StringBuilder();

               
                result.ForEach(x => inserts.AppendLine($"insert into {Schema}.Cargasiniciales (FechaCarga,FechaProceso,Folio,DvPensionado,FechaSolicitud,FechaEfectiva ) values ('{DateTime.Now}','{x.FechaProceso}','{x.Folio}','{x.DvPensionado}','{x.FechaSolicitud}','{x.FechaEfectiva}');"));

                connection.Execute(inserts.ToString(), null, null, 240);
            }
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
