using Dapper;
using DocumentFormat.OpenXml.Drawing;
using Galvarino.Web.Common;
using Galvarino.Web.data.migrations.pensionado;
using Galvarino.Web.Data;
using Galvarino.Web.Data.Repository.Pensionado;
using Galvarino.Web.Models.Application;
using Galvarino.Web.Models.Application.Pensionado;
using Galvarino.Web.Models.Mappings;
using Galvarino.Web.Services.Notification;
using Galvarino.Web.Services.Workflow;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TinyCsvParser;
using CargasIniciales = Galvarino.Web.Models.Application.Pensionado.CargasIniciales;

namespace Galvarino.Web.Workers
{
    internal class CargaInicialPensionado : BackgroundService
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
        private readonly ICargaInicialRepository _cargainicialrepository;

        public CargaInicialPensionado(ICargaInicialRepository cargainicialrepository, ILogger<CargaInicialPensionado> logger, IServiceProvider services, IConfiguration configuration, INotificationKernel mailService)
        {
            _logger = logger;
            _configuration = configuration;
            _mailService = mailService;
            _scope = services.CreateScope();
            _cargainicialrepository = cargainicialrepository;

            this.setHora();
        }

        public CargaInicialPensionado()
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

            var _context = _scope.ServiceProvider.GetRequiredService<PensionadoDbContext>();
            string rutaDescargar = _configuration.GetValue<string>("RutaOportunidadPensionado");// @"C:\Desarrollos\Archivos\Galvarino\";
            string nombreArchivoOportunidad = _configuration.GetValue<string>("ArchivoOportunidadPensionado");// "Aux_OportunidadesPensionados.txt";
            string rutaDesafilliacionPensionado = _configuration.GetValue<string>("RutaDesafilliacionPensionado");// @"C:\Desarrollos\Archivos\Galvarino\";
            string nombreArchivoDesafilliacion = _configuration.GetValue<string>("ArchivoDesafilliacionPensionado");// "DESAFILIACION_PENSIONADO.csv";
            string Schema = _configuration.GetValue<string>("schemaPensionado");
            Common.Utils utils = new Common.Utils();

            using (var connection = new SqlConnection(_configuration.GetConnectionString("DocumentManagementConnection")))
            {
                string sql = "";
                int existeCarga;

                DateTime Dia = Convert.ToDateTime(DateTime.Today.ToString("yyyy-MM-dd"));
                Output outCarga = new Output();


                CargasInicialesEstado cie = new CargasInicialesEstado();
                CsvParserOptions csvParserOptions = new CsvParserOptions(true, ';');
                string archivo = null;
                CsvParser<CargaInicialPensionadoDesafiliacionIM> csvParserD;
                CsvParser<CargaInicialPensionadoIM> csvParserA;


                #region carga_desafiliacion
                var existe = _context.CargasInicialesEstado.Where(p => p.NombreArchivoCarga == nombreArchivoDesafilliacion && p.FechaCarga.Date >= Dia).FirstOrDefault();

                //if (existe == null)
                //{
                //    cie = new CargasInicialesEstado();
                //    cie.NombreArchivoCarga = nombreArchivoDesafilliacion;
                //    cie.Estado = "DesafiliacionParcial";
                //    cie.FechaCarga = Convert.ToDateTime(Dia);
                //    archivo = rutaDesafilliacionPensionado + nombreArchivoDesafilliacion;

                //    CargaInicialPensionadoDesafiliacionMapping csvMapper = new CargaInicialPensionadoDesafiliacionMapping();
                //    csvParserD = new CsvParser<CargaInicialPensionadoDesafiliacionIM>(csvParserOptions, csvMapper);

                //    var result = csvParserD
                //        .ReadFromFile(archivo, Encoding.ASCII)
                //        .Where(x => x.IsValid)
                //        .Select(x => x.Result)
                //        .AsSequential()
                //        .ToList();

                //    StringBuilder inserts = new StringBuilder();
                //    cie = CargaEstado(cie, nombreArchivoDesafilliacion, Dia);
                //    result.ForEach(x => inserts.AppendLine($"('{cie.Id}','{DateTime.Now}','{x.FechaProceso}', N'{x.Folio}', N'{x.Estado}', N'{x.RutPensionado}', N'{x.DvPensionado}', N'{x.NombrePensionado}', N'{x.IdTipo}', " +
                //                                               $"'{x.FechaSolicitud}', '{x.FechaEfectiva}', '{x.IdSucursal.Replace("O ", "")}', N'{utils.QuitaAcento(x.Tipo)}', N'DESAFILIACION'),"));

                //    //string estadoCarga = _cargainicialrepository.CargaAfiliaciones();
                //    CargaInicial(Schema, connection, inserts);

                //}
                //else
                //{
                //    Debug.WriteLine("Ya existe carga de [" + nombreArchivoDesafilliacion + "] con fecha [" + DateTime.Today.ToString("yyyy-MM-dd") + "]");
                //}
                //outCarga = CargaPensionado(nombreArchivoDesafilliacion, connection, Dia);
                #endregion

                //_cargainicialrepository.CargaAfiliaciones();


                #region carga_oportunidad
                existe = _context.CargasInicialesEstado.Where(p => p.NombreArchivoCarga == nombreArchivoOportunidad && p.FechaCarga.Date >= Dia).FirstOrDefault();

                if (existe == null)
                {

                    cie = new CargasInicialesEstado();
                    cie.NombreArchivoCarga = nombreArchivoOportunidad;
                    cie.Estado = "OportunidadParcial";
                    cie.FechaCarga = Convert.ToDateTime(Dia);
                    archivo = rutaDescargar + nombreArchivoOportunidad;

                    CargaInicialPensionadoMapping csvMapper = new CargaInicialPensionadoMapping();
                    csvParserA = new CsvParser<CargaInicialPensionadoIM>(csvParserOptions, csvMapper);
                    var result = csvParserA
                        .ReadFromFile(archivo, Encoding.ASCII)
                        .Where(x => x.IsValid)
                        .Select(x => x.Result)
                        .AsSequential()
                        .ToList();

                    cie = CargaEstado(cie, nombreArchivoOportunidad, Dia);

                    StringBuilder inserts = new StringBuilder();
                    //result.ForEach(x => inserts.AppendLine($"('{cie.Id}','{DateTime.Now}','{x.FechaProceso}', N'{x.Folio}', N'{x.Estado}', N'{x.RutPensionado}', N'{x.DvPensionado}', N'{x.NombrePensionado + " " + x.Nombre2Pensionado + " " + x.ApellidoPatPensionado + " " + x.ApellidoMatPensionado}', N'{x.IdTipo}', " +
                    //                                           $"'{x.FechaSolicitud}', '{x.FechaEfectiva}', '{x.IdSucursal.Replace("O ", "")}', N'{utils.QuitaAcento(x.Tipo)}', N'AFILIACION'),"));

                    //CargaInicial(Schema, connection, inserts);
                    // INtento de carga con Entity framework
                    foreach (var ci in result)
                    {

                        var cargaInicialEstado = new CargasInicialesEstado()
                        {
                            Id = cie.Id
                        };

                       
                        var tipo = _context.Tipo.Where(t => t.Id == ci.IdTipo.ToString()).FirstOrDefault();

                        var sucursal = _context.Sucursal.Where(t => t.Id == Int32.Parse(ci.IdSucursal.Replace("O ", ""))).FirstOrDefault();
                        

                       

                        var caragaInicial = new CargasIniciales()
                        {
                            FechaCarga = DateTime.Now,
                            FechaProceso = ci.FechaProceso,
                            Folio = ci.Folio,
                            Estado = ci.Estado,
                            RutPensionado = ci.RutPensionado,
                            DvPensionado = ci.DvPensionado,
                            NombrePensionado = ci.NombrePensionado + " " + ci.Nombre2Pensionado + " " + ci.ApellidoPatPensionado + " " + ci.ApellidoMatPensionado,
                            Tipo = tipo,
                            FechaSolicitud = ci.FechaSolicitud,
                            FechaEfectiva = ci.FechaEfectiva,
                            Sucursal = sucursal,
                            Forma = utils.QuitaAcento(ci.Tipo),
                            TipoMovimiento = "AFILIACION"

                        };

                        _context.CargasIniciales.Add(caragaInicial);
                        _context.SaveChanges();



                    }


                }
                else
                {
                    Debug.WriteLine("Ya existe carga de [" + nombreArchivoOportunidad + "] con fecha [" + DateTime.Today.ToString("yyyy-MM-dd") + "]");
                }
                #endregion

                //outCarga = CargaPensionado( nombreArchivoOportunidad, connection, Dia);

                //Debug.WriteLine("salida carga pensionado ["+ outCarga.Mensaje + "]");

                //outCarga = CargaExpediente(null, connection, Dia);
                //Debug.WriteLine("salida carga expedientes [" + outCarga.Mensaje + "]");


            }
            

           
        }


        private void CargaInicial(string Schema, SqlConnection connection, StringBuilder inserts)
        {

            string subInsert = "INSERT INTO " + Schema + ".Cargasiniciales";
            string campos = "(CargaInicialEstadoId,FechaCarga, FechaProceso, Folio,Estado, RutPensionado, DvPensionado, NombrePensionado, TipoID,FechaSolicitud, FechaEfectiva, SucursalId, Forma, TipoMovimiento) ";
            string valores = inserts.ToString().Substring(0, inserts.ToString().Length - 3) + ";";
            string insert = subInsert + campos + " VALUES " + valores;

            connection.Execute(insert, null, null, 240);

        }

        private CargasInicialesEstado CargaEstado(CargasInicialesEstado cie, string nombreArchivo, DateTime Dia)
        {

            var _context = _scope.ServiceProvider.GetRequiredService<PensionadoDbContext>();

            _context.CargasInicialesEstado.Add(cie);
            _context.SaveChanges();

            cie = _context.CargasInicialesEstado.Where(p => p.NombreArchivoCarga == nombreArchivo && p.FechaCarga.Date >= Dia).FirstOrDefault();
            return cie;

        }

        private Output CargaPensionado(string nombreArchivo, SqlConnection connection, DateTime Dia) {

            var _context = _scope.ServiceProvider.GetRequiredService<PensionadoDbContext>();
            Common.Utils utils = new Common.Utils();

            Output output = new Output();

            var cargaInicials = new List<CargasIniciales>();

            try
            {

                var existe = _context.CargasInicialesEstado.Where(p => p.NombreArchivoCarga == nombreArchivo && p.FechaCarga.Date >= Dia).FirstOrDefault();

                if (existe != null)
                {
                    cargaInicials = _context.CargasIniciales.Where(o => o.FechaCarga.Date >= Dia &&  o.CargaInicialEstado.Id == existe.Id && (o.Estado == "Ganada" || o.Estado == "Aprobado")).Include(t => t.Tipo).Include(s => s.Sucursal).Include(c => c.CargaInicialEstado).ToList();

                    foreach (var ci in cargaInicials)
                    {
                    
                        var numTicket = utils.GeneraTicket("02");
                        var pensionado = new Pensionado()
                        {
                            Folio = ci.Folio,
                            FechaFormaliza = Dia,
                            RutCliente = ci.RutPensionado + "-" + ci.DvPensionado,
                            NombreCliente = ci.NombrePensionado,
                            NumeroTicket = numTicket,
                            TipoPensionado = ci.Tipo.Id
                        };

                        var existeCarga = _context.Pensionado.Where(o => o.Folio == pensionado.Folio && o.RutCliente == pensionado.RutCliente).FirstOrDefault();

                        if (existeCarga == null)
                        {
                            _context.Pensionado.Add(pensionado);
                            _context.SaveChanges();
                        }
                    
                   
                    }
                }
                output.Exito = true;
                output.Mensaje = "Datos Cargado Con Exito";
                output.codigo = "10";

            }
            catch (Exception ex)
            {
                output.Exito = true;
                output.Mensaje = "Error Al Cargar Datos";
                output.codigo = "00";

            }

            return output;

        }

        private Output CargaExpediente( string nombreArchivo, SqlConnection connection, DateTime Dia) {
            Output output = new Output();
            var _context = _scope.ServiceProvider.GetRequiredService<PensionadoDbContext>();

            var pensionados = new List<Pensionado>();

            try
            {
                pensionados = _context.Pensionado.Where(p =>p.FechaFormaliza >= Dia).ToList();

                foreach (var p in pensionados)
                {

                    var existe = _context.Expedientes.Where(e => e.PensionadoId == p.Id).FirstOrDefault();
                    if (existe == null)
                    {
                        var sucursal = _context.CargasIniciales.FirstOrDefault(c => c.Folio == p.Folio);
                        var expediente = new Expedientes()
                        {
                            FechaCreacion = Dia,
                            PensionadoId = p.Id,
                            TipoExpediente = 0,
                            IdSucursalActividad = sucursal.Sucursal.Id.ToString()
                        };


                        _context.Expedientes.Add(expediente);
                        _context.SaveChanges();

                    }
                }

                output.Exito = true;
                output.Mensaje = "Datos Cargado Con Exito";
                output.codigo = "00";
            }
            catch (Exception ex)
            {
                output.Exito = true;
                output.Mensaje = "Error Al Cargar Datos";
                output.codigo = "00";

            }
        

            return output;

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


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {

            object state = null;
            _timer = new Timer(DoWork, state, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }
    }
}
