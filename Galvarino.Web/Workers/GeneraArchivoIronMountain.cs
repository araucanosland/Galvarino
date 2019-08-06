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
    internal class GeneraArchivoIronMountain : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceScope _scope;
        private Timer _timer;
        private bool estaOcupado = false;
        private readonly TimeSpan horaInicial = new TimeSpan(22, 0, 0);
        private readonly TimeSpan horaFinal = new TimeSpan(23, 59, 59);
        private IEnumerable<string> registrosArchivoIM;
        
        public GeneraArchivoIronMountain(ILogger<GeneraArchivoIronMountain> logger, IServiceProvider services, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _scope = services.CreateScope();
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
            /*
                Generar el Cierre de Cajas
                1.- Correr Script de Cierre de Cajas

                Generar Archivo para Iron Mountain

                1.- detectar todos los documentos que esten en la etapa de despacho a custodia
                2.- generar el archivo con los documentos encontrados
                3.- subir el archivo a ftp
                4.- mover los documentos en el workflow

            */

            /* Antes que todo se debe revisar 
                1.- Que la hora actual esta entre las 23:10 y 23:59 
                2.- Que el servicio no este ocupado haciendo un trabajo
                3.- manejar la ejecucion diaria y que no se vuelva a repetir la tarea un dia (podria sobreescribir datos y nos deja la FOX papá)  
            */

            var cantidadTablaControl = 0;
            using (var connection = new SqlConnection(_configuration.GetConnectionString("DocumentManagementConnection")))
            {
                var sqlControlArchivo = @"
                        SELECT count(*) Total
                        FROM Tareas b
                        INNER JOIN Solicitudes c on b.SolicitudId = c.Id
                        INNER JOIN Creditos d on c.NumeroTicket = d.NumeroTicket
                        INNER JOIN ExpedientesCreditos e on d.Id = e.CreditoId
                        INNER JOIN Etapas f on b.EtapaId = f.Id
                        INNER JOIN (
                            select distinct cv.CodigoSeguimiento, pvv.FolioCredito, cv.Usuario
                            from CajasValoradas cv
                            inner join PasosValijasValoradas pvv on cv.CodigoSeguimiento  = pvv.CodigoCajaValorada
                            where cv.MarcaAvance in ('READYTOPROCESS-', 'READYTOPROCESS')
                        ) caja on d.FolioCredito = caja.FolioCredito

                        where b.Estado = 'Activada'
                        and b.EtapaId = 16
                ";
                cantidadTablaControl = connection.Query<int>(sqlControlArchivo).FirstOrDefault();
            }

            var horaActual = DateTime.Now.TimeOfDay;
            if(horaActual >= horaInicial && horaActual <= horaFinal && !estaOcupado && cantidadTablaControl > 0)
            {
                /*Ponemos al servicio en modo ocupado */
                estaOcupado = true;

                /* Comenzamos con el cierre de cajas */
                _logger.LogInformation("Iniciando el proceso de Cierre de cajas.");

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DocumentManagementConnection")))
                {

                    var sqlCierreCajas = @"             
                        truncate table TareasFinalizarCustodia
                        insert into TareasFinalizarCustodia
                        SELECT b.Id TareaId,  b.SolicitudId, caja.Usuario, caja.FolioCredito, caja.CodigoSeguimiento, c.NumeroTicket
                        FROM Tareas b
                        INNER JOIN Solicitudes c on b.SolicitudId = c.Id
                        INNER JOIN Creditos d on c.NumeroTicket = d.NumeroTicket
                        INNER JOIN ExpedientesCreditos e on d.Id = e.CreditoId
                        INNER JOIN Etapas f on b.EtapaId = f.Id
                        INNER JOIN (
                            select distinct cv.CodigoSeguimiento, pvv.FolioCredito, cv.Usuario
                            from CajasValoradas cv
                            inner join PasosValijasValoradas pvv on cv.CodigoSeguimiento  = pvv.CodigoCajaValorada
                            where cv.MarcaAvance in ('READYTOPROCESS-', 'READYTOPROCESS')
                        ) caja on d.FolioCredito = caja.FolioCredito

                        where b.Estado = 'Activada'
                        and b.EtapaId = 16

                        update a 
                        set a.EjecutadoPor = b.Usuario, a.Estado = 'Finalizada', a.FechaTerminoFinal = GETDATE()
                        from Tareas a
                        inner join TareasFinalizarCustodia b on a.Id = b.TareaId

                        insert into Tareas(SolicitudId, EtapaId, AsignadoA, ReasignadoA, EjecutadoPor, Estado, FechaInicio, FechaTerminoEstimada, FechaTerminoFinal, UnidadNegocioAsignada)
                        select SolicitudId, 17 EtapaId, 'Custodia' AsignadoA, null rEA, null Ejec, 'Activada' estado, GETDATE() fechaini, null fecterestimada, null fectermfinal, null una 
                        from TareasFinalizarCustodia

                        update a
                        set a.CajaValoradaId = d.Id
                        from ExpedientesCreditos a
                        inner join Creditos b on a.CreditoId = b.Id
                        inner join TareasFinalizarCustodia c on b.FolioCredito = c.FolioCredito
                        inner join CajasValoradas d on c.CodigoSeguimiento = d.CodigoSeguimiento

                        delete from PasosValijasValoradas
                        where CodigoCajaValorada in (
                            select distinct cv.CodigoSeguimiento
                            from CajasValoradas cv
                            inner join PasosValijasValoradas pvv on cv.CodigoSeguimiento  = pvv.CodigoCajaValorada
                            where cv.MarcaAvance in ('READYTOPROCESS-', 'READYTOPROCESS')
                        )

                        update CajasValoradas  set MarcaAvance = 'DESPACUST'
                        where MarcaAvance in ('READYTOPROCESS-', 'READYTOPROCESS')
                    ";

                    var registrosCierreCajas = connection.Execute(sqlCierreCajas, null, commandType: CommandType.Text);

                    var sqlArchivo = @"
                    select a.FolioCredito + ';' + a.RutCliente + ';' +  case when a.TipoCredito = 0 then 'Nuevo' when a.TipoCredito = 1 then 'Reprogramacion' when a.TipoCredito = 2 then 'AcuerdoPago' end Data
                    from Creditos a
                    inner join TareasFinalizarCustodia b on a.FolioCredito = b.FolioCredito";

                    registrosArchivoIM = connection.Query<string>(sqlArchivo).ToList();

                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("FolioCredito;RutCliente;TipoCredito");
                foreach (var registro in registrosArchivoIM)
                {
                    sb.AppendLine(registro);
                }

                string nombreArchivo = "galvarino" + DateTime.Now.ToString("ddMMyyyy", CultureInfo.InvariantCulture) + ".txt";
                string archivoSalida = @"C:\galvarino\envios_ftp\" + nombreArchivo;
                //string archivoEntrada = @"C:\galvarino\entradas_ftp\Rpt_LA_CRED_Recepcionados.csv";
                StreamWriter escritorArchivo = new StreamWriter(archivoSalida);
                escritorArchivo.Write(sb.ToString());
                escritorArchivo.Close();
                string host = "www.imrmconnect.cl";
                string username = "usr_sftp_cs410_base";
                string password = "sftp%20@18CS(410";
                var connectionInfo = new ConnectionInfo(host, "sftp", new PasswordAuthenticationMethod(username, password));
                using (var sftp = new SftpClient(connectionInfo))
                {
                    sftp.Connect();
                    //sftp.ChangeDirectory("/MyFolder");
                    using (var uplfileStream = File.OpenRead(archivoSalida))
                    {
                        sftp.UploadFile(uplfileStream, nombreArchivo, true);
                    }
                    sftp.Disconnect();
                }


            }else{
                _logger.LogInformation("No estamos dentro del rango de horas, el servicio eta ocupado o ya corrio para el dia de hoy.");
            }

            

            


            
        }
    }
}
