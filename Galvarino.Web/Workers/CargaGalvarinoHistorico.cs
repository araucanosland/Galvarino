
using Dapper;
using Galvarino.Web.Data;
using Galvarino.Web.Models.Application;
using Galvarino.Web.Models.Helper;
using Galvarino.Web.Models.Mappings;
using Galvarino.Web.Services;
using Galvarino.Web.Services.Notification;
using Galvarino.Web.Services.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TinyCsvParser;

namespace Galvarino.Web.Workers
{
    public class CargaGalvarinoHistorico : BackgroundService
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
        private IEnumerable<string> registrosArchivoIM;
        private IEnumerable<string> foliosCajasCerrar;

        public CargaGalvarinoHistorico(ILogger<CargaGalvarinoHistorico> logger, IServiceProvider services, IConfiguration configuration, INotificationKernel mailService)
        {
            _logger = logger;
            _configuration = configuration;
            _mailService = mailService;
            _scope = services.CreateScope();
            this.setHora();
        }
        public CargaGalvarinoHistorico()
        {

        }

        public override void Dispose() => _timer?.Dispose();

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Timed Background Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
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
        private void DoWork(object state)
        {

            var _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            string Schema = _configuration.GetValue<string>("schema");
            string nombreArchivo;
            //nombreArchivo = "Carga";
            string rutaDescargar;
            int registrosCargados = 0;
            StringBuilder mailTemplate = new StringBuilder();// se genera string correo noticación
            StringBuilder foliosRepetidos = new StringBuilder();//se genera string para folio repetidos
            string sql = "";
            string ruta = "";


            using (var connection = new SqlConnection(_configuration.GetConnectionString("DocumentManagementConnection")))
            {
                var horaActual = DateTime.Now.TimeOfDay;
                if (horaActual >= horaInicial && horaActual <= horaFinal && !estaOcupado)
                {
                    /*Todo:  Revisar Findesemanas*/
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                    {
                        nombreArchivo = "Carga" + DateTime.Now.AddDays(-3).ToString("ddMMyyyy");
                        ruta = _configuration["RutaCargaCredito"] + nombreArchivo + ".txt";
                    }
                    else if (
                        DateTime.Now.DayOfWeek == DayOfWeek.Thursday ||
                        DateTime.Now.DayOfWeek == DayOfWeek.Wednesday ||
                        DateTime.Now.DayOfWeek == DayOfWeek.Tuesday ||
                        DateTime.Now.DayOfWeek == DayOfWeek.Friday
                    )
                    {
                        nombreArchivo = "Carga" + DateTime.Now.AddDays(-1).ToString("ddMMyyyy");
                        ruta = _configuration["RutaCargaCredito"] + nombreArchivo + ".txt";
                    }
                    else
                    {
                        return;
                    }



                    if (!estaOcupado)
                    {
                        estaOcupado = true;

                        var creditos = new List<dynamic>();
                        sql = @"select FolioCredito,s.NumeroTicket,t.id tareaId
                             from Creditos c
                            ,ExpedientesCreditos ec
                            , Solicitudes s
                            ,Tareas t
                                where c.Id = ec.CreditoId
                            and ec.CajaValoradaId = 717
                            and s.NumeroTicket = c.NumeroTicket
                            and t.SolicitudId=s.Id
                            and t.Estado='Activada'";

                        creditos = connection.Query<dynamic>(sql).AsList();

                        foreach (var c in creditos)
                        {
                           string updateSql = "update Tareas"+
                                  " set EtapaId = 12"+
                                  " ,asignadoA = 'Mesa Control'"+
                                  " where Id ="+c.tareaId;

                            connection.Execute(updateSql.ToString(), null, null, 240);

                        }


                    }

                      


                       
                    
                }

            }// fin estado ocupado
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
