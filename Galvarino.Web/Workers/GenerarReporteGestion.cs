using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Galvarino.Web.Workers
{
    internal class GenerarReporteGestion : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private Timer _timer;
        private readonly TimeSpan horaInicial = new TimeSpan(22, 0, 0);
        private readonly TimeSpan horaFinal = new TimeSpan(23, 59, 59);

        private readonly IConfiguration _configuration;
        private bool estaOcupado = false;

        public GenerarReporteGestion(ILogger<GenerarReporteGestion> logger, Timer timer, IConfiguration configuration)
        {
            _logger = logger;
            _timer = timer;
            _configuration = configuration;

        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is starting.");
            object state = null;
            _timer = new Timer(DoWork, state, TimeSpan.Zero, TimeSpan.FromMinutes(10));
            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken)
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
                estaOcupado = true;


            }

        }

        
    }
}
