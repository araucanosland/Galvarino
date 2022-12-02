using ClosedXML.Excel;
using Galvarino.Web.Data;
using Galvarino.Web.Data.Repository;
using Galvarino.Web.Models.Application;
using Galvarino.Web.Services.Notification;
using Galvarino.Web.Services.Workflow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Galvarino.Web.Workers
{
    internal class GenerarReporteGestion : BackgroundService
    {
        
        private readonly ISolicitudRepository _solicitudRepository;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly INotificationKernel _mailService;
        private readonly IServiceScope _scope;
        private Timer _timer;
        private bool estaOcupado = false;
        private TimeSpan horaInicial;
        private TimeSpan horaFinal;


        public GenerarReporteGestion(ILogger<GenerarReporteGestion> logger, ISolicitudRepository solicitudRepository, IServiceProvider services, IConfiguration configuration, INotificationKernel mailService)
        {
            
            _solicitudRepository = solicitudRepository;
            _logger = logger;
            _configuration = configuration;
            _mailService = mailService;
            _scope = services.CreateScope();
            this.setHora();
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
            var _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var horaActual = DateTime.Now.TimeOfDay;
            if (horaActual >= horaInicial && horaActual <= horaFinal && !estaOcupado)
            {
                try
                {
                    string fecha = DateTime.Now.ToString("dd-MM-yyyy");
                    DateTime fechaActual = Convert.ToDateTime(fecha);


                    //revisar y modificar para que traiga el ultimo registro no solo el activo
                    List<ReporteProgramado> reporte = _context.ReporteProgramado.Where(x => x.Estado == "Pendiente" && x.FechaEjecucion == fechaActual).ToList();

                    if (reporte.Count() != 0)
                    {
                        foreach (ReporteProgramado rep in reporte)
                        {

                            string nombreArchivo = "REPORTE_CREDITOS_GALVARINO" + "_" + rep.RutUsuario + ".xlsx";
                            string ruta = _configuration["RutaCargaReporteProgramado"];
                            string rutacompleta = ruta + nombreArchivo;
                            if (!Directory.Exists(ruta))
                                Directory.CreateDirectory(ruta);

                            if (System.IO.File.Exists(rutacompleta))
                                System.IO.File.Delete(rutacompleta);

                            DateTime fechainicial = Convert.ToDateTime(rep.FechaInicio);
                            DateTime fechafinal = Convert.ToDateTime(rep.FechaFinal);

                            DataTable data = _solicitudRepository.ObtenerDataReporte(fechainicial, fechafinal);

                            //var salida = _solicitudRepository.ReporteGestion(fechainicial, fechafinal);

                            using (var libro = new XLWorkbook())
                            {
                                data.TableName = "Reporte_" + DateTime.Now.ToString("M");
                                var hoja = libro.Worksheets.Add(data);
                                hoja.ColumnsUsed().AdjustToContents();

                                using (var memoria = new MemoryStream())
                                {
                                    //libro.SaveAs(memoria);
                                    libro.SaveAs(rutacompleta);
                                    //var nombreExcel = "REPORTE_CREDITOS_GALVARINO.xlsx";
                                    //return File(memoria.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreExcel);
                                }
                            }
                            rep.Estado = "Finalizado";
                            _context.ReporteProgramado.Update(rep);
                            _context.SaveChangesAsync();
                        }

                    }

                    
                    ResultadoBase resultado = new ResultadoBase();
                    resultado.Estado = "Ok";
                    resultado.Mensaje = "Reporte descargado en ruta";
                    resultado.Objeto = "";
                   
                }
                catch (Exception ex)
                {
                    ResultadoBase resultado = new ResultadoBase();
                    resultado.Estado = "Error";
                    resultado.Mensaje = ex.Message;
                    resultado.Objeto = "";

                    
                }

            }

        }



        private void setHora()
        {
            var rawMomentoInicio = _configuration.GetValue<string>("CoordinacionWorkers:ReporteGetionWorker:HoraInicio");
            int hInicio = Convert.ToInt32(rawMomentoInicio.Split(':')[0]);
            int mInicio = Convert.ToInt32(rawMomentoInicio.Split(':')[1]);
            int sInicio = Convert.ToInt32(rawMomentoInicio.Split(':')[2]);

            this.horaInicial = new TimeSpan(hInicio, mInicio, sInicio);

            var rawMomentoFin = _configuration.GetValue<string>("CoordinacionWorkers:ReporteGetionWorker:HoraFin");
            int hFin = Convert.ToInt32(rawMomentoFin.Split(':')[0]);
            int mFin = Convert.ToInt32(rawMomentoFin.Split(':')[1]);
            int sFin = Convert.ToInt32(rawMomentoFin.Split(':')[2]);

            this.horaFinal = new TimeSpan(hFin, mFin, sFin);

        }


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Background Service is starting.");
            object state = null;
            _timer = new Timer(DoWork, state, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }
    }
}
