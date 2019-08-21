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
using Galvarino.Web.Services.Notification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace Galvarino.Web.Workers
{
    internal class CargaInicialWorker : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly INotificationKernel _mailService;
        private readonly IServiceScope _scope;
        private Timer _timer;
        private bool estaOcupado = false;
        private readonly TimeSpan horaInicial = new TimeSpan(22, 0, 0);
        private readonly TimeSpan horaFinal = new TimeSpan(23, 59, 59);
        private IEnumerable<string> registrosArchivoIM;
        private IEnumerable<string> foliosCajasCerrar;

        public CargaInicialWorker(ILogger<CargaInicialWorker> logger, IServiceProvider services, IConfiguration configuration, INotificationKernel mailService)
        {
            _logger = logger;
            _configuration = configuration;
            _mailService = mailService;
            _scope = services.CreateScope();
        }

        public CargaInicialWorker()
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
                        

            var horaActual = DateTime.Now.TimeOfDay;
            if (horaActual >= horaInicial && horaActual <= horaFinal && !estaOcupado)
            {
                /*Ponemos al servicio en modo ocupado */
                estaOcupado = true;
                /* Comenzamos con el cierre de cajas */
                _logger.LogInformation("Iniciando el proceso.");

            }
            else
            {
                _logger.LogInformation("No estamos dentro del rango de horas, el servicio eta ocupado o ya corrio para el dia de hoy.");
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Background Service is starting.");
            object state = null;
            _timer = new Timer(DoWork, state, TimeSpan.Zero, TimeSpan.FromMinutes(10));
            return Task.CompletedTask;
        }
    }
}
