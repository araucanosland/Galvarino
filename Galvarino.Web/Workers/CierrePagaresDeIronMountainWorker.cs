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
using Galvarino.Web.Models.Mappings;
using Galvarino.Web.Services.Notification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using TinyCsvParser;

namespace Galvarino.Web.Workers
{
    internal class CierrePagaresDeIronMountainWorker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceScope _scope;
        private readonly INotificationKernel _mailService;
        private Timer _timer;
        private bool estaOcupado = false;
        private TimeSpan horaInicial;
        private TimeSpan horaFinal;
        private IEnumerable<string> registrosArchivoIM;
        
        public CierrePagaresDeIronMountainWorker(ILogger<CierrePagaresDeIronMountainWorker> logger, IServiceProvider services, IConfiguration configuration, INotificationKernel mailService)
        {
            _logger = logger;
            _configuration = configuration;
            _scope = services.CreateScope();
            _mailService = mailService;
            this.setHora();
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

        private void setHora()
        {
            var rawMomentoInicio = _configuration.GetValue<string>("CoordinacionWorkers:CierrePagaresDeIronMountainWorker:HoraInicio");
            int hInicio = Convert.ToInt32(rawMomentoInicio.Split(':')[0]);
            int mInicio = Convert.ToInt32(rawMomentoInicio.Split(':')[1]);
            int sInicio = Convert.ToInt32(rawMomentoInicio.Split(':')[2]);

            this.horaInicial = new TimeSpan(hInicio, mInicio, sInicio);

            var rawMomentoFin = _configuration.GetValue<string>("CoordinacionWorkers:CierrePagaresDeIronMountainWorker:HoraFin");
            int hFin = Convert.ToInt32(rawMomentoFin.Split(':')[0]);
            int mFin = Convert.ToInt32(rawMomentoFin.Split(':')[1]);
            int sFin = Convert.ToInt32(rawMomentoFin.Split(':')[2]);

            this.horaFinal = new TimeSpan(hFin, mFin, sFin);

        }

        private async void DoWork(object state)
        {
            /*
                Obtener los documentos necesarios para cierre de Cajas y finalizar wf

                1. Obtener archivos de Iron Mountain
                2. Leer Archivos
            */



            var horaActual = DateTime.Now.TimeOfDay;
            if (horaActual >= horaInicial && horaActual <= horaFinal && !estaOcupado)
            {
                /*Ponemos al servicio en modo ocupado */
                estaOcupado = true;
                string rutaDescargar = _configuration.GetValue<string>("RutasWorkers:ArchivoCreditosRecibidosIM");
                _logger.LogInformation("Iniciando el proceso de Cierre de Workflows.");

                string host = "www.imrmconnect.cl";
                string username = "usr_sftp_cs410_base";
                string password = "sftp%20@18CS(410";
                ConnectionInfo connectionInfo = new ConnectionInfo(host, "sftp", new PasswordAuthenticationMethod(username, password));
                using (SftpClient sftp = new SftpClient(connectionInfo))
                {
                    sftp.Connect();
                    sftp.ChangeDirectory("reportes");
                    //var sftpFileInfo = sftp.GetStatus(@"Rpt_LA_CRED_Recepcionados.csv");

                    using (Stream fileStream = File.Create(rutaDescargar))
                    {
                        sftp.DownloadFile("Rpt_LA_CRED_Recepcionados.csv", fileStream);
                    }
                    sftp.Disconnect();
                }

                CsvParserOptions csvParserOptions = new CsvParserOptions(true, ';');
                CreditosRecibidosMapping csvMapper = new CreditosRecibidosMapping();
                CsvParser<CreditosRecibidosIM> csvParser = new CsvParser<CreditosRecibidosIM>(csvParserOptions, csvMapper);

                var result = csvParser
                    .ReadFromFile(rutaDescargar, Encoding.ASCII)
                    .Where(x => x.IsValid)
                    .Select(x => x.Result)
                    .AsSequential()
                    .ToList();

                StringBuilder inserts = new StringBuilder();
                result.ForEach(x => inserts.AppendLine($"insert into dbo.RecepcionadosIM values ('{x.Folio}');"));

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DocumentManagementConnection")))
                {
                    //_logger.LogInformation($"La cantidad es: {result.Count.ToString()}");

                    string limpiezas = @"
                        truncate table dbo.RecepcionadosIM;
                        truncate table dbo.TareasFinalizarWF;";

                    await connection.ExecuteAsync(limpiezas, null, null, 240);

                    await connection.ExecuteAsync(inserts.ToString(), null, null, 240);
                    string tareasAFinalizar = @" insert into dbo.TareasFinalizarWF
                                    SELECT b.Id TareaId,  b.SolicitudId, c.NumeroTicket, d.FolioCredito, b.EtapaId
                                    FROM Tareas b
                                    INNER JOIN Solicitudes c on b.SolicitudId = c.Id
                                    INNER JOIN Creditos d on c.NumeroTicket = d.NumeroTicket
                                    INNER JOIN ExpedientesCreditos e on d.Id = e.CreditoId
                                    INNER JOIN Etapas f on b.EtapaId = f.Id
                                    INNER JOIN RecepcionadosIM im on d.FolioCredito = im.Folio
                                    where b.Estado = 'Activada'
                                    and b.EtapaId <> 18";

                    await connection.ExecuteAsync(tareasAFinalizar.ToString(), null, null, 240);

                    string cerrarEtapas = @"update a 
                                            set a.EjecutadoPor = 'wfboot', a.Estado = 'Finalizada', a.FechaTerminoFinal = GETDATE()
                                            from Tareas a
                                            inner join TareasFinalizarWF b on a.Id = b.TareaId";

                    await connection.ExecuteAsync(cerrarEtapas, null, null, 240);


                    string abrirEtapaFinal = @"insert into Tareas(SolicitudId, EtapaId, AsignadoA, ReasignadoA, EjecutadoPor, Estado, FechaInicio, FechaTerminoEstimada, FechaTerminoFinal, UnidadNegocioAsignada)
                                                select SolicitudId, 18 EtapaId, 'wfboot' AsignadoA, null rEA, null Ejec, 'Activada' estado, GETDATE() fechaini, null fecterestimada, null fectermfinal, null una 
                                                from TareasFinalizarWF";

                    await connection.ExecuteAsync(abrirEtapaFinal, null, null, 240);
                }


                var destinatarios = _configuration.GetSection("CoordinacionWorkers:CierrePagaresDeIronMountainWorker:DestinatariosNotificaciones").Get<string[]>();
                StringBuilder mailTemplate = new StringBuilder();
                mailTemplate.AppendLine("<p>Los Pagaré Ingresados en IM, han sido cerrados</p>");
                mailTemplate.AppendLine("<small>Correo enviado automaticamente por Galvarino favor no contestar.</small>");
                await _mailService.SendEmail(destinatarios, "Cierre de Pagarés de IM", mailTemplate.ToString());


            }
            else
            {
                _logger.LogInformation("No estamos dentro del rango de horas, el servicio eta ocupado o ya corrio para el dia de hoy.");
            }

        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Execute Async invocado");
            return Task.CompletedTask;
        }
    }
}
