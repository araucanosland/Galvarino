﻿using ClosedXML.Excel;
using Dapper;
using Galvarino.Web.Data;
using Galvarino.Web.Models.Application;
using Galvarino.Web.Services.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
                estaOcupado = true;
                var reporte = await _context.ReporteProgramado.Where(x => x.Estado == "Pendiente" && x.FechaEjecucion == fechaActual).ToListAsync();

                if (reporte != null)
                {
                    foreach (ReporteProgramado rep in reporte)
                    {
                        string nombreArchivo = _configuration["NombreArchivoReporteProgramado"] + "_" + rep.RutUsuario + ".xlsx";
                        string rutalocal = _configuration["RutaDescargaLocalReporteProgramado"];
                        string rutacompleta = rutalocal + nombreArchivo;
                        if (!Directory.Exists(rutalocal))
                            Directory.CreateDirectory(rutalocal);

                        if (System.IO.File.Exists(rutacompleta))
                            System.IO.File.Delete(rutacompleta);

                        DateTime fechaInicio = Convert.ToDateTime(rep.FechaInicio);
                        DateTime fechaFinal = Convert.ToDateTime(rep.FechaFinal);

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
                                    ROW_NUMBER() over (partition by cr.foliocredito order by ci.FechaCarga) as rnk,FORMAT(getdate(),'yyyyMM') as PERIODO
                                    ,format(getdate(),'dd/MM/yyyy') as FECHA_PROC,cr.FolioCredito as Folio_Credito, LEFT( cr.RutCliente,CHARINDEX('-', cr.RutCliente)-1) as RUT_AFILIADO
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
                                    and ci.FechaCorresponde >= convert(datetime,@fechaInicio)
                                    and ci.FechaCorresponde <= convert(datetime,@fechaFinal)
                                    )reporte
                                    where reporte.rnk=1
                                    ";


                            var parametros = new
                            {
                                @fechaInicio = fechaInicio,
                                @fechaFinal = fechaFinal
                                
                            };

                            var aux = await connection.QueryAsync<dynamic>(sql, parametros, null,360,null);
                            DataTable data = Newtonsoft.Json.JsonConvert.DeserializeObject<DataTable>(Newtonsoft.Json.JsonConvert.SerializeObject(aux.ToList()));

                            using (var libro = new XLWorkbook())
                            {
                                data.TableName = "Reporte_" + DateTime.Now.ToString("M");
                                var hoja = libro.Worksheets.Add(data);
                                hoja.ColumnsUsed().AdjustToContents();

                                using (var memoria = new MemoryStream())
                                {
                                    libro.SaveAs(rutacompleta);                                  
                                }
                            }

                            string user = _configuration["UserFTP"];
                            string pass = _configuration["PasswordFTP"];
                            FtpWebRequest dirFtp = ((FtpWebRequest)FtpWebRequest.Create(_configuration["RutaFTP"] ));

                            NetworkCredential cr = new NetworkCredential(user, pass);
                            dirFtp.Credentials = cr;
                            dirFtp.UsePassive = true;
                            dirFtp.UseBinary = true;
                            dirFtp.KeepAlive = true;

                            dirFtp.Method = WebRequestMethods.Ftp.ListDirectory;
                            WebResponse response = dirFtp.GetResponse();
                            StreamReader reader = new StreamReader(response.GetResponseStream());
                            string file = reader.ReadLine();
                            while (file != null)
                            {
                                if (file.Equals(nombreArchivo))
                                {
                                    dirFtp = ((FtpWebRequest)FtpWebRequest.Create(_configuration["RutaFTP"] + "/" + nombreArchivo));
                                    NetworkCredential crdelete = new NetworkCredential(user, pass);
                                    dirFtp.Credentials = crdelete;
                                    dirFtp.UsePassive = true;
                                    dirFtp.UseBinary = true;
                                    dirFtp.KeepAlive = true;

                                    dirFtp.Method = WebRequestMethods.Ftp.DeleteFile;
                                    WebResponse responsedelete = dirFtp.GetResponse();
                                }
                                file = reader.ReadLine();
                            }


                            //grabar

                             dirFtp = ((FtpWebRequest)FtpWebRequest.Create(_configuration["RutaFTP"] + "/" + nombreArchivo));

                             cr = new NetworkCredential(user, pass);
                            dirFtp.Credentials = cr;
                            dirFtp.UsePassive = true;
                            dirFtp.UseBinary = true;
                            dirFtp.KeepAlive = true;
                            dirFtp.Method = WebRequestMethods.Ftp.UploadFile;
                            FileStream stream = File.OpenRead(rutacompleta);
                            byte[] buffer = new byte[stream.Length];
                            stream.Read(buffer, 0, buffer.Length);
                            stream.Close();
                            Stream reqStream = dirFtp.GetRequestStream();
                            reqStream.Write(buffer, 0, buffer.Length);
                            reqStream.Flush();
                            reqStream.Close();

                            rep.Estado = "Finalizado";
                            _context.ReporteProgramado.Update(rep);
                            await _context.SaveChangesAsync();
                        }
                    }

                }

            }
            estaOcupado = false;

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
