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
    internal class CargaInicialWorker : BackgroundService
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

        public CargaInicialWorker(ILogger<CargaInicialWorker> logger, IServiceProvider services, IConfiguration configuration, INotificationKernel mailService)
        {
            _logger = logger;
            _configuration = configuration;
            _mailService = mailService;
            _scope = services.CreateScope();
            this.setHora();
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

            var _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            string Schema = _configuration.GetValue<string>("schema");
            string nombreArchivo;
            //nombreArchivo = "Carga";
            string rutaDescargar;
            int registrosCargados = 0;
            StringBuilder mailTemplate = new StringBuilder();// se genera string correo noticaci�n
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
                        DateTime.Now.DayOfWeek == DayOfWeek.Friday ||
                         DateTime.Now.DayOfWeek == DayOfWeek.Sunday
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



                    //rutaDescargar = @"c:\cargainicial\Carga29062021.txt";
                    //nombreArchivo = "Carga29062021";
                    nombreArchivo = "Carga" + DateTime.Now.AddDays(-1).ToString("ddMMyyyy");
                    ruta = _configuration["RutaCargaCredito"] + nombreArchivo + ".txt";

                    //Valida si archivo de carga y dia de hoy estan cargados en BASE
                    int existeCarga;
                    sql = "select count(*) from " + Schema + ".CargasInicialesEstado" +
                    " /*where convert(varchar, fechaCarga,112)= convert(varchar, getdate(), 112)*/" +
                    " where NombreArchivoCarga = '" + nombreArchivo + "'" +
                    "and Estado='CargadoTotal'";
                    existeCarga = connection.Query<int>(sql).FirstOrDefault();

                    IEnumerable<CargaInicial> configur;

                    if (existeCarga == 0)
                    {
                        if (!estaOcupado)
                        {

                            sql = "select count(*) from " + Schema + ".CargasInicialesEstado" +
                            " where convert(varchar, fechaCarga,112)= convert(varchar, getdate(), 112)" +
                            " and NombreArchivoCarga = '" + nombreArchivo + "'" +
                            "and Estado='PendienteParcial'";
                            existeCarga = connection.Query<int>(sql).FirstOrDefault();

                            estaOcupado = true;

                            if (existeCarga == 0)
                            {
                                CsvParserOptions csvParserOptions = new CsvParserOptions(true, ';');
                                CargaInicialMapping csvMapper = new CargaInicialMapping();
                                CsvParser<CargaInicialIM> csvParser = new CsvParser<CargaInicialIM>(csvParserOptions, csvMapper);

                                var result = csvParser
                                    .ReadFromFile(rutaDescargar, Encoding.ASCII)
                                    .Where(x => x.IsValid)
                                    .Select(x => x.Result)
                                    .AsSequential()
                                    .ToList();
                                StringBuilder inserts = new StringBuilder();
                                //  result.ForEach(x => inserts.AppendLine($"insert into {Schema}.Cargasiniciales values ('{DateTime.Now}','{DateTime.ParseExact(x.FechaCorresponde.ToString(), "ddMMyyyy", CultureInfo.InvariantCulture)}','{x.FolioCredito}','{x.RutAfiliado}','{x.CodigoOficinaIngreso}','{x.CodigoOficinaPago}','{x.LineaCredito}','{x.RutResponsable}','{x.CanalVenta}','{x.Estado}','{x.FechaCorresponde}','{nombreArchivo}','{x.TipoSegmento}','{x.NroOferta}','{x.SeguroCesantia}','{x.Afecto}','{x.Aval}','{x.SeguroDesgravamen}','{x.TipoVenta}','{x.FormaPago}','{x.CompraCartera}','{x.DigitalizarSegDesgr}','{x.DigitalizarSegCesantia}');"));

                                result.ForEach(x => inserts.AppendLine($"insert into {Schema}.Cargasiniciales values ('{DateTime.Now}','{DateTime.ParseExact(x.FechaCorresponde.ToString(), "ddMMyyyy", CultureInfo.InvariantCulture)}','{x.FolioCredito}','{x.RutAfiliado}','{x.CodigoOficinaIngreso}','{x.CodigoOficinaPago}','{x.LineaCredito}','{x.RutResponsable}','{x.CanalVenta}','{x.Estado}','{x.FechaCorresponde}','{nombreArchivo}','{x.SeguroCesantia}','{x.Afecto}','{x.Aval}','{x.SeguroDesgravamen}','');"));

                                connection.Execute(inserts.ToString(), null, null, 240);

                                string insertarCargasInincialesEstado = @"
                                  insert into CargasInicialesEstado(fechacarga,NombreArchivoCarga,Estado)  
                                  values (getdate(),'" + nombreArchivo + "','PendienteParcial' )";
                                connection.Execute(insertarCargasInincialesEstado.ToString(), null, null, 240);


                                //    //----------- Fin carga de Registros en Cargas Iniciales

                            }
                            //---------- SE cuenta Cantidad de Registros cargados en Cargas Iniciales

                            var cargaInicials = new List<CargaInicial>();
                            sql = "select * from CargasIniciales where  NombreArchivoCarga = '" + nombreArchivo + "' and CodigoOficinaIngreso not in ('A610')";

                            cargaInicials = connection.Query<CargaInicial>(sql).AsList();
                            //-------------Se genera Registro de Carga 
                            //StringBuilder mailTemplate = new StringBuilder();
                            mailTemplate.AppendLine("<p>Los cr�ditos han sido cargados exitosamente</p>");
                            mailTemplate.AppendLine("<p>REPORTE DE CARGA DIARIA GALVARINO</p> ");
                            mailTemplate.AppendLine("<p>---------------------------------</p>");
                            mailTemplate.AppendLine("<p>Fecha de Carga: " + DateTime.Now + "</p>");
                            mailTemplate.AppendLine("<p> \n</p>");
                            mailTemplate.AppendLine("<p> \n</p>");
                            mailTemplate.AppendLine("<p> \n</p>");
                            mailTemplate.AppendLine("<p>* Nombre Archivo : " + nombreArchivo + ".txt</p>");
                            // mailTemplate.AppendLine("<p>* Archivo Base Carga " + result.Count + " Registro(s)</p>");
                            mailTemplate.AppendLine("<p> \n</p>");
                            mailTemplate.AppendLine("<p>* Carga en Tabla CargasInicales " + cargaInicials.Count + " Registro(s)</p>");



                            foreach (var ci in cargaInicials)
                            {

                                //Se valida si credito ya esta cargado en BD
                                sql = "select count(*)" +
                                " from " + Schema + ".creditos" +
                                " where FolioCredito='" + ci.FolioCredito + "'";
                                int existe = connection.Query<int>(sql).FirstOrDefault();


                                //Valida que No est� cargado anteriormente el folio
                                if (existe == 0)
                                {
                                    //-----------SE inicia creacion de objeto para cargas en Tablas creditos.

                                    var oficinaProceso = _context.Oficinas.Include(x => x.OficinaProceso).FirstOrDefault(x => x.Codificacion == ci.CodigoOficinaPago);
                                    string esRM = oficinaProceso.EsRM ? $"1" : $"0";

                                    /*  TODO: Caso de La Uni�n ver con Jenny Bernales  */
                                    Dictionary<string, string> _setVariables = new Dictionary<string, string>();
                                    _setVariables.Add("OFICINA_PAGO", ci.CodigoOficinaPago);
                                    _setVariables.Add("OFICINA_INGRESO", ci.CodigoOficinaIngreso);
                                    _setVariables.Add("FOLIO_CREDITO", ci.FolioCredito);
                                    _setVariables.Add("RUT_AFILIADO", ci.RutAfiliado);
                                    _setVariables.Add("FECHA_VENTA", ci.FechaCorresponde.ToString());
                                    _setVariables.Add("ES_RM", esRM);
                                    _setVariables.Add("DOCUMENTO_LEGALIZADO", $"0");
                                    _setVariables.Add("OFICINA_PROCESA_NOTARIA", oficinaProceso.OficinaProceso.Codificacion);

                                    //---------Se Genera Registo en Tareas y Solicitudes
                                    _wfservice = new WorkflowService(new DefaultWorkflowKernel(_context, _configuration));
                                    var wf = _wfservice.Instanciar(ProcesoDocumentos.NOMBRE_PROCESO, "wfboot", "Ingreso Automatico de Creditos Vendidos", _setVariables);


                                    Credito cred = new Credito
                                    {
                                        FechaDesembolso = ci.FechaCorresponde,
                                        FechaFormaliza = DateTime.Now.AddDays(-1),
                                        FolioCredito = ci.FolioCredito,
                                        MontoCredito = 0,
                                        RutCliente = ci.RutAfiliado,
                                        NumeroTicket = wf.NumeroTicket
                                    };

                                    if (ci.LineaCredito.ToLower().Contains("credito normal") && ci.Estado.Contains("Reprogramado"))
                                    {
                                        cred.TipoCredito = TipoCredito.Reprogramacion;
                                    }
                                    else if (ci.LineaCredito.ToLower().Contains("credito normal") || ci.LineaCredito.ToLower().Contains("compra cartera") || ci.LineaCredito.ToLower().Contains("credito paralelo"))
                                    {
                                        cred.TipoCredito = TipoCredito.Normal;
                                    }
                                    else if (ci.LineaCredito.ToLower().Contains("reprogr"))
                                    {
                                        cred.TipoCredito = TipoCredito.Reprogramacion;
                                    }
                                    else if (ci.LineaCredito.ToLower().Contains("acuerdo de creditos castigados"))
                                    {
                                        cred.TipoCredito = TipoCredito.AcuerdoPago;
                                    }
                                    else if (ci.LineaCredito.ToLower().Contains("COVID"))
                                    {
                                        cred.TipoCredito = TipoCredito.Reprogramacion;
                                    }


                                    IEnumerable<ConfiguracionDocumento> configs = _context.ConfiguracionDocumentos.Where(x => x.TipoCredito == cred.TipoCredito && x.TipoExpediente == TipoExpediente.Legal).ToList();


                                    ExpedienteCredito expcred = new ExpedienteCredito
                                    {
                                        Credito = cred,
                                        FechaCreacion = DateTime.Now,
                                        TipoExpediente = TipoExpediente.Legal,
                                    };

                                    int incrementor = 1;
                                    foreach (var confItem in configs)
                                    {
                                        Documento docmnt = new Documento
                                        {
                                            TipoDocumento = confItem.TipoDocumento,
                                            Codificacion = confItem.Codificacion,
                                            Resumen = confItem.TipoDocumento.ToString("D")
                                        };


                                        if (confItem.Codificacion == "01")//pagaré
                                        {

                                            expcred.Documentos.Add(docmnt);
                                        }
                                        if (confItem.Codificacion == "02")//CI
                                        {

                                            expcred.Documentos.Add(docmnt);
                                        }
                                        if (confItem.Codificacion == "06")//Acuerdo de pago
                                        {

                                            expcred.Documentos.Add(docmnt);
                                        }
                                        if (confItem.Codificacion == "07")//Reprogramaciones
                                        {

                                            expcred.Documentos.Add(docmnt);
                                        }

                                        //if (confItem.Codificacion == "09")//Seguro Desgravamen
                                        //{
                                        //    if (ci.SeguroDesgravamen == "1")
                                        //        expcred.Documentos.Add(docmnt);
                                        //}

                                        //if (confItem.Codificacion == "10")//Seguro Cesantia
                                        //{
                                        //    if (ci.SeguroCesantia == "1")
                                        //        expcred.Documentos.Add(docmnt);
                                        //}





                                        incrementor++;
                                    }
                                    _context.ExpedientesCreditos.Add(expcred);
                                }
                                else
                                {
                                    // se elimina registro de tabla carga inicial para no se incluida en cargas de Workflow

                                    //  sql = "delete   CargasIniciales" +
                                    //" where FolioCredito = '" + ci.FolioCredito + "'" +
                                    //" and CONVERT(VARCHAR, fechaCarga, 112) = CONVERT(VARCHAR, GETDATE(), 112)" +
                                    //" and NombreArchivoCarga = '" + nombreArchivo + "'";

                                    // connection.Execute(sql);

                                    foliosRepetidos.AppendLine("<p>Folio Repetido: " + ci.FolioCredito + "</p>");

                                }
                            }
                            //  _context.SaveChangesAsync();


                        }
                        _context.SaveChanges();
                    }
                    if (existeCarga == 0)
                    {
                        string UpdateCargasInincialesEstado = @"
                                                  Update  CargasInicialesEstado
                                                   set Estado='CargadoTotal'  
                                                   where NombreArchivoCarga='" + nombreArchivo + "'";
                        connection.Execute(UpdateCargasInincialesEstado.ToString(), null, null, 240);
                    }
                    estaOcupado = false;
                }

            }
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
            _logger.LogInformation("Timed Background Service is starting.");
            object state = null;
            _timer = new Timer(DoWork, state, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }
    }
}
