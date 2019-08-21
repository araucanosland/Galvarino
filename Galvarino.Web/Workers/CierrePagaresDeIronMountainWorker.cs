using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace Galvarino.Web.Workers
{
    internal class CierrePagaresDeIronMountainWorker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceScope _scope;
        private Timer _timer;
        private bool estaOcupado = false;
        private readonly TimeSpan horaInicial = new TimeSpan(22, 0, 0);
        private readonly TimeSpan horaFinal = new TimeSpan(23, 59, 59);
        private IEnumerable<string> registrosArchivoIM;
        
        public CierrePagaresDeIronMountainWorker(ILogger<CierrePagaresDeIronMountainWorker> logger, IServiceProvider services, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _scope = services.CreateScope();
        }

        public CierrePagaresDeIronMountainWorker()
        {
        }

        public override void Dispose()
        {
            _timer?.Dispose();
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is starting.");
            object state = null;
            _timer = new Timer(DoWork, state, TimeSpan.Zero, TimeSpan.FromMinutes(10));
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            /*
                Obtener los documentos necesarios para cierre de Cajas y finalizar wf

                1. Obtener archivos de Iron Mountain
                2. Leer Archivos
            */


            var horaActual = DateTime.Now.TimeOfDay;
            if(horaActual >= horaInicial && horaActual <= horaFinal && !estaOcupado)
            {
                /*Ponemos al servicio en modo ocupado */
                estaOcupado = true;

                _logger.LogInformation("Iniciando el proceso de Cierre de Workflows.");

                string host = "www.imrmconnect.cl";
                string username = "usr_sftp_cs410_base";
                string password = "sftp%20@18CS(410";
                var connectionInfo = new ConnectionInfo(host, "sftp", new PasswordAuthenticationMethod(username, password));
                using (var sftp = new SftpClient(connectionInfo))
                {
                    sftp.Connect();
                    sftp.ChangeDirectory("reportes");
                    var sftpFileInfo = sftp.GetStatus(@"Rpt_LA_CRED_Recepcionados.csv");
                    var localFileInfo = new FileInfo(@"C:\galvarino\entradas_ftp\Rpt_LA_CRED_Recepcionados.csv");
                    if(sftpFileInfo.BlockSize > (ulong)localFileInfo.Length){
                        using (Stream fileStream = File.Create(@"C:\galvarino\entradas_ftp\Rpt_LA_CRED_Recepcionados.csv"))
                        {
                            sftp.DownloadFile("Rpt_LA_CRED_Recepcionados.csv", fileStream);
                        }
                    }
                    
                    sftp.Disconnect();
                }



            }else{
                _logger.LogInformation("No estamos dentro del rango de horas, el servicio eta ocupado o ya corrio para el dia de hoy.");
            }

            
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
