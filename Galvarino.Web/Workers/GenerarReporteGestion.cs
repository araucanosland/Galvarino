using Dapper;
using Galvarino.Web.Data;
using Galvarino.Web.Models.Application;
using Galvarino.Web.Services.Notification;
using Galvarino.Web.Services.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Galvarino.Web.Workers
{
    internal class GenerarReporteGestion : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly INotificationKernel _mailService;
        private readonly IServiceScope _scope;
        private Timer _timer;
        private bool estaOcupado = false;
        private TimeSpan horaInicial;
        private TimeSpan horaFinal;


        public GenerarReporteGestion(ILogger<GenerarReporteGestion> logger, IServiceProvider services, IConfiguration configuration, INotificationKernel mailService)
        {
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

        private async void DoWork(object state)
        {
            var _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var horaActual = DateTime.Now.TimeOfDay;
            string fecha = DateTime.Now.ToString("dd-MM-yyyy");
            DateTime fechaActual = Convert.ToDateTime(fecha);

            if (horaActual >= horaInicial && horaActual <= horaFinal && !estaOcupado)
            {
                var reporte = await _context.ReporteProgramado.Where(x => x.Estado == "Pendiente" && x.FechaEjecucion == fechaActual).ToListAsync();

                if (reporte != null)
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

                        //DataTable aux = new DataTable();
                       
                        //DataTable data = _solicitudRepository.ObtenerDataReporte(fechainicial, fechafinal);

                        using (var connection = new SqlConnection(_configuration.GetConnectionString("DocumentManagementConnection")))
                       
                        {
                            string sql = @"
                            select reporte.PERIODO,reporte.FECHA_PROC,reporte.Folio_Credito,reporte.FECHA_COLOCACION,
                            reporte.RUT_AFILIADO,reporte.DV_RUT_AFILIADO,reporte.TIPO_CREDITO,reporte.ID_OFICINA_EVALUACION,reporte.OFICINA_EVALUACION,
                            reporte.ID_OFICINA_PAGO,reporte.OFICINA_PAGO,reporte.ID_OFICINA_LEGALIZACION,reporte.OFICINA_LEGALIZACION,
                            reporte.DOCUMENTO_REQUERIDO_1,
                            (case  when reporte.DOCUMENTO_REQUERIDO_1='Pagare' then 'Fotocopia Cedula Identidad' end) DOCUMENTO_REQUERIDO_2,
                            reporte.ID_ESTADO_FOLIO_GALVARINO,reporte.ESTADO_FOLIO_GALVARINO,reporte.FECHA_ETAPA_ACTUAL,
                            reporte.AREA_RESP_ETAPA,reporte.FOLIO_SKP,reporte.CODIGO_VALIJA,reporte.TIPO_VENTA
                            from (
                                select  
                                    ROW_NUMBER() over (partition by cr.foliocredito order by ci.FechaCarga) as rnk,FORMAT(ci.FechaCarga,'yyyyMM') as PERIODO
                                    ,ci.FechaCarga as FECHA_PROC,cr.FolioCredito as Folio_Credito, LEFT( cr.RutCliente,CHARINDEX('-', cr.RutCliente)-1) as RUT_AFILIADO
                                    ,ci.FechaCorresponde as FECHA_COLOCACION,RIGHT(cr.RutCliente,1) as DV_RUT_AFILIADO,tc.TipoCredito as TIPO_CREDITO
                                    ,ci.CodigoOficinaIngreso as ID_OFICINA_EVALUACION 
                                    ,(select nombre from dbo.Oficinas where ci.CodigoOficinaIngreso = Codificacion) as OFICINA_EVALUACION
                                    ,ci.CodigoOficinaPago as ID_OFICINA_PAGO
                                    ,(select nombre from dbo.Oficinas where ci.CodigoOficinaPago = Codificacion) as OFICINA_PAGO
                                    ,(select codificacion from dbo.Oficinas where ofi.OficinaProcesoId = Id) as ID_OFICINA_LEGALIZACION
                                    ,(select nombre from dbo.Oficinas where ofi.OficinaProcesoId = Id) as OFICINA_LEGALIZACION
                                    ,(  case when do.Codificacion = 01 then 'Pagare' when do.Codificacion= 07 then 'Acuerdo de pago' when do.Codificacion = 06 then 'Hoja Prolongacion'  end) as DOCUMENTO_REQUERIDO_1
                                    ,(case when do.Codificacion = 02 then 'Fotocopia Cedula Identidad' end) as DOCUMENTO_REQUERIDO_2
                                    ,et.id as ID_ESTADO_FOLIO_GALVARINO
                                    ,et.Nombre as ESTADO_FOLIO_GALVARINO
                                    ,ta.FechaInicio as FECHA_ETAPA_ACTUAL
                                    ,ta.AsignadoA as AREA_RESP_ETAPA
                                    ,cv.CodigoSeguimiento as FOLIO_SKP
                                    ,vv.CodigoSeguimiento as CODIGO_VALIJA
                                    ,(case when ci.TipoVenta = 01 or ci.TipoVenta = 04 or ci.TipoVenta = 05 then 'Venta Remota' end) as TIPO_VENTA
                                    from [dbo].[Tareas] ta
                                    inner join [dbo].[Etapas] et on ta.EtapaId = et.Id
                                    inner join [dbo].[Solicitudes] sl on ta.SolicitudId = sl.Id
                                    inner join [dbo].[Creditos] cr on sl.NumeroTicket = cr.NumeroTicket
                                    inner join [dbo].[ExpedientesCreditos] ex on cr.Id = ex.CreditoId
                                    inner join [dbo].[TipoCredito] tc on cr.TipoCredito = tc.Id
                                    inner join [dbo].[CargasIniciales] ci on ci.FolioCredito = cr.FolioCredito
                                    inner join [dbo].[Documentos] do on do.ExpedienteCreditoId = ex.Id
                                    inner join [dbo].[Oficinas] ofi on ofi.Codificacion = ci.CodigoOficinaPago
                                    left join [dbo].[CajasValoradas] cv on ex.CajaValoradaId = cv.Id
                                    left join [dbo].[ValijasValoradas] vv on ex.ValijaValoradaId = vv.Id
                                    where ta.Estado = 'Activada'
                                    and ci.FechaCorresponde >= '" + fechainicial + @"'/*'2022-08-18 00:00:00.0000000' */
                                    and ci.FechaCorresponde <= '" + fechafinal + @"'/*'2022-09-19 00:00:00.0000000'*/
                                    )reporte
                                    where reporte.rnk=1
                                    ";
                            var aux = connection.Query<string>(sql);
                            DataTable data = Newtonsoft.Json.JsonConvert.DeserializeObject<DataTable>(Newtonsoft.Json.JsonConvert.SerializeObject(aux.ToList()));
                        }
                    }

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
            _timer = new Timer(DoWork, state, TimeSpan.Zero, TimeSpan.FromMinutes(10));
            return Task.CompletedTask;
        }
    }
}
