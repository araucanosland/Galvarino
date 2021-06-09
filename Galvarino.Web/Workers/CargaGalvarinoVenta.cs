using Dapper;
using Galvarino.Web.Data;
using Galvarino.Web.Services.Notification;
using Galvarino.Web.Services.Workflow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Galvarino.Web.Workers
{
    public class CargaGalvarinoVenta : BackgroundService
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

        public CargaGalvarinoVenta(ILogger<CargaGalvarinoVenta> logger, IServiceProvider services, IConfiguration configuration, INotificationKernel mailService)
        {
            _logger = logger;
            _configuration = configuration;
            _mailService = mailService;
            _scope = services.CreateScope();
            this.setHora();
        }

        public CargaGalvarinoVenta()
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
                    rutaDescargar = _configuration.GetValue<string>("RutaCargaCredito") + nombreArchivo + ".txt";


                    var salida = new List<dynamic>();
                    //Valida si archivo de carga y dia de hoy estan cargados en BASE
                    int existeCarga;
                    sql = @"
                                                SELECT  c.*
                          FROM [GALVARINO].[dbo].[mesa] t
                          ,Creditos c
                          where c.FolioCredito=t.folio";
                    salida = connection.Query<dynamic>(sql).AsList();

                    foreach (var sa in salida)
                    {
                        string actualizar = " declare @p_id_solicitud int" +
                     " declare @p_unidadNegocio varchar(5)" +
                     " declare @p_id_etapa int" +
                     " declare @p_existe int" +
                     " set @p_unidadNegocio=(select top 1 UnidadNegocioAsignada " +
                     " from Tareas a inner" +
                     " join Solicitudes b on a.SolicitudId = b.Id" +
                     " where b.NumeroTicket = '" + sa.NumeroTicket + "'" +
                     " and UnidadNegocioAsignada is not null" + " order by 1 asc) " +
                     " set @p_id_solicitud = (select id from Solicitudes where NumeroTicket = '" + sa.NumeroTicket + "')" +
                     " set @p_existe =(select COUNT(*) from Tareas where SolicitudId=@p_id_solicitud and EtapaId=18) " +
                      " update a" +
                     " set a.EjecutadoPor = 'wfboot', a.Estado = 'Finalizada', a.FechaTerminoFinal = GETDATE()" +
                     " from Tareas a inner" +
                     " join Solicitudes b on a.SolicitudId = b.Id and a.estado='Activada'" +
                     " where b.NumeroTicket = '" + sa.NumeroTicket + "'" +
                     " insert into Tareas(SolicitudId, EtapaId, AsignadoA, ReasignadoA, EjecutadoPor, Estado, FechaInicio, FechaTerminoEstimada, FechaTerminoFinal, UnidadNegocioAsignada)  " +
                     " values           (@p_id_solicitud, 12,    'Mesa Control', null, null, 'Activada', GETDATE(), null, null, null) ";
                    

                        connection.Execute(actualizar.ToString(), null, null, 240);
                    }

                }
                
              
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
