using Dapper;
using DocumentFormat.OpenXml.Drawing;
using Galvarino.Web.Common;
using Galvarino.Web.data.migrations.pensionado;
using Galvarino.Web.Data;
using Galvarino.Web.Models.Application;
using Galvarino.Web.Models.Application.Pensionado;
using Galvarino.Web.Models.Mappings;
using Galvarino.Web.Models.Workflow;
using Galvarino.Web.Services.Notification;
using Galvarino.Web.Services.Workflow;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using OfficeOpenXml.Style;
using Remotion.Linq.Clauses;
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
       
        public CargaInicialPensionado(ILogger<CargaInicialPensionado> logger, IServiceProvider services, IConfiguration configuration, INotificationKernel mailService)
        {
            _logger = logger;
            _configuration = configuration;
            _mailService = mailService;
            _scope = services.CreateScope();
           

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

        private string rutaDescargar = null;
        private string nombreArchivoOportunidad = null;
        private string rutaDesafilliacionPensionado = null;
        private string nombreArchivoDesafilliacion = null;


        private void DoWork(object state)
        {
            
            DateTime Dia = Convert.ToDateTime(DateTime.Today.ToString("yyyy-MM-dd"));

            rutaDescargar = _configuration.GetValue<string>("RutaOportunidadPensionado");// @"C:\Desarrollos\Archivos\Galvarino\";
            nombreArchivoOportunidad = _configuration.GetValue<string>("ArchivoOportunidadPensionado");// "Aux_OportunidadesPensionados.txt";
            rutaDesafilliacionPensionado = _configuration.GetValue<string>("RutaDesafilliacionPensionado");// @"C:\Desarrollos\Archivos\Galvarino\";
            nombreArchivoDesafilliacion = _configuration.GetValue<string>("ArchivoDesafilliacionPensionado");// "DESAFILIACION_PENSIONADO.csv";
            var okExpediente = "";
            var okSolicitud = "";
            var _context = _scope.ServiceProvider.GetRequiredService<PensionadoDbContext>();
            var iniciaProceso = false;
            Output outCarga = new Output();

            RegistraLog(new Output("INICIO CARGA ARCHIVOS PENSIONADO", "CARGA PENSIONADO", "CARGA PENSIONADO", "INICIO", "Inicio Proceso"));
            try
            {
               outCarga = CargaInicial(Dia);
            }
            catch (Exception ex)
            {
                RegistraLog(new Output("Erro Al Leer Archivo e Intentar cargar Archivo "+ex, "CARGA PENSIONADO", "CARGA PENSIONADO", "INICIO", "Inicio Proceso"));
            }
            var cargasInicialesPendientes = _context.CargasInicialesEstado.Where(p => p.FechaCarga.Date >= Dia && p.Procesado == 0).ToList();

            var cantidad = cargasInicialesPendientes.Count();
            if (cargasInicialesPendientes.Count() >= 1) 
                { 
                    iniciaProceso = true;
                }
            
            if (iniciaProceso)
            {
                outCarga = CargaPensionado(Dia);
                if (outCarga.Codigo == "00" || outCarga.Codigo == "01")
                {
                    outCarga = CargaExpediente(Dia);
                }
                if (outCarga.Codigo == "00" || outCarga.Codigo == "01")
                {
                    outCarga = CargaSolicitud(Dia);
                }
                if (outCarga.Codigo == "00" || outCarga.Codigo == "01")
                {
                    outCarga = CargaDocumento(Dia);
                }
                if (outCarga.Codigo == "00" || outCarga.Codigo == "01")
                {
                    RegistraLog(new Output("CARGA ARCHIVOS PENSIONADO PROCESADA", "CARGA PENSIONADO", "CARGA PENSIONADO", "FIN", "FinProceso"));
                }
            }
            else {

                RegistraLog(new Output("SIN DATOS A PROCESAR DE ARCHIVOS PENSIONADO", "CARGA PENSIONADO", "CARGA PENSIONADO", "FIN", "FinProceso"));

            }
            

        }
        #region Carga tabla CargaInicial
        private Output CargaInicial(DateTime Dia)
        {
            Output outCarga = new Output();
            var _context = _scope.ServiceProvider.GetRequiredService<PensionadoDbContext>();
           
            Common.Utils utils = new Common.Utils();

            string sql = "";
            int existeCarga;

           
            CargasInicialesEstado cie = new CargasInicialesEstado();
            CsvParserOptions csvParserOptions = new CsvParserOptions(true, ';');
            string archivo = null;
            CsvParser<CargaInicialPensionadoDesafiliacionIM> csvParserD;
            CsvParser<CargaInicialPensionadoIM> csvParserA;

            #region carga_desafiliacion
            var existe = _context.CargasInicialesEstado.Where(p => p.NombreArchivoCarga == nombreArchivoDesafilliacion && p.FechaCarga.Date >= Dia).FirstOrDefault();

            if (existe == null)
            {
                cie = new CargasInicialesEstado();
                cie.NombreArchivoCarga = nombreArchivoDesafilliacion;
                cie.Estado = "DesafiliacionParcial";
                cie.FechaCarga = Convert.ToDateTime(Dia);
                cie.Procesado = 0;
                archivo = rutaDesafilliacionPensionado + nombreArchivoDesafilliacion;

                CargaInicialPensionadoDesafiliacionMapping csvMapper = new CargaInicialPensionadoDesafiliacionMapping();
                csvParserD = new CsvParser<CargaInicialPensionadoDesafiliacionIM>(csvParserOptions, csvMapper);

                var result = csvParserD
                    .ReadFromFile(archivo, Encoding.ASCII)
                    .Where(x => x.IsValid)
                    .Select(x => x.Result)
                    .AsSequential()
                    .ToList();

                cie = CargaEstado(cie, nombreArchivoDesafilliacion, Dia);

                foreach (var ci in result)
                {
                    try
                    {
                        var tipo = _context.Tipo.Where(t => t.Id == ci.IdTipo.ToString()).FirstOrDefault();
                        var sucursal = _context.Sucursal.Where(t => t.Id == Int32.Parse(ci.IdSucursal.Replace("O ", ""))).FirstOrDefault();
                        var fechaProc = utils.formatearFecha(ci.FechaProceso, "YYYYMMDD");


                        var caragaInicial = new CargasIniciales()
                        {
                            CargaInicialEstado = cie,
                            FechaCarga = DateTime.Now,
                            FechaProceso = DateTime.Parse(fechaProc),
                            Folio = ci.Folio,
                            Estado = ci.Estado,
                            RutPensionado = ci.RutPensionado,
                            DvPensionado = ci.DvPensionado,
                            NombrePensionado = ci.NombrePensionado,
                            Tipo = tipo,
                            FechaSolicitud = ci.FechaSolicitud,
                            FechaEfectiva = ci.FechaEfectiva,
                            Sucursal = sucursal,
                            Forma = utils.QuitaAcento(ci.Tipo),
                            TipoMovimiento = "DESAFILIACION"

                        };

                        _context.CargasIniciales.Add(caragaInicial);
                        _context.SaveChanges();

                        outCarga = new Output("Datos Cargados Con Exito", "00", "CargaDesafiliacion", ci.Folio, "Folio");

                    }
                    catch (Exception ex)
                    {
                        outCarga = new Output("Error Al Cargar Datos :" + ex, "10", "CargaDesafiliacion", ci.Folio, "Folio");

                    }
                    if (!outCarga.Mensaje.Equals(null))
                    {
                        RegistraLog(outCarga);
                    }
                }
                outCarga = new Output("Datos Cargados Con Exito", "00", "CargaDesafiliacion", "FIN", "Fin Proceso");
            }
            else
            {
                outCarga = new Output("Dato Se Encontraba Cargado", "01", "CargaDesafiliacion", nombreArchivoDesafilliacion, "NombreArchivo");
            }

            RegistraLog(outCarga);

            #endregion

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

                foreach (var ci in result)
                {
                    try
                    {
                        var tipo = _context.Tipo.Where(t => t.Id == ci.IdTipo.ToString()).FirstOrDefault();
                        var sucursal = _context.Sucursal.Where(t => t.Id == Int32.Parse(ci.IdSucursal.Replace("O ", ""))).FirstOrDefault();
                        var caragaInicial = new CargasIniciales()
                        {
                            CargaInicialEstado = cie,
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
                        outCarga = new Output("Datos Cargados Con Exito", "00", "CargaAfiliacion", ci.Folio, "Folio");

                    }
                    catch (Exception ex)
                    {
                        outCarga = new Output("Error Al Cargar Datos :" + ex, "10", "CargaAfiliacion", ci.Folio, "Folio");
                    }
                    if (outCarga.Mensaje != null)
                    {
                        RegistraLog(outCarga);
                    }
                }
                outCarga = new Output("Datos Cargados Con Exito", "00", "CargaAfiliacion", "Fin Proceso", "FinProceso");

            }
            else
            {
                outCarga = new Output("Dato Se Encontraba Cargado", "01", "CargaAfiliacion", nombreArchivoOportunidad, "NombreArchivo");
            }

            RegistraLog(outCarga);

            #endregion

            return outCarga;
        }
        #endregion
       
        #region Carga tabla CargaInicialEstado
        private CargasInicialesEstado CargaEstado(CargasInicialesEstado cie, string nombreArchivo, DateTime Dia)
        {
            Output outCarga = new Output();
            try { 
                var _context = _scope.ServiceProvider.GetRequiredService<PensionadoDbContext>();

                _context.CargasInicialesEstado.Add(cie);
                _context.SaveChanges();

                cie = _context.CargasInicialesEstado.Where(p => p.NombreArchivoCarga == nombreArchivo && p.FechaCarga.Date >= Dia).FirstOrDefault();

                outCarga = new Output("Datos Cargados Con Exito", "00", "CargasInicialesEstado", "Fin Proceso["+ nombreArchivo + "]", "NombreArchivo");
            }
            catch (Exception ex)
            {                
                outCarga = new Output("Error Al Cargar Datos :" + ex, "10", "CargasInicialesEstado", "Fin Proceso[" + nombreArchivo + "]", "NombreArchivo");
            }
            if (outCarga.Mensaje != null)
            {
                RegistraLog(outCarga);
            }

            return cie;
        }
        #endregion

        #region Carga tabla Pensionado
        private Output CargaPensionado(DateTime Dia) {

            var _context = _scope.ServiceProvider.GetRequiredService<PensionadoDbContext>();
            Common.Utils utils = new Common.Utils();
            Output output = new Output();
            var cargaInicials = new List<CargasIniciales>();
            var procesados = 0;
            try
            {

                var cargasInicialesEstado  = _context.CargasInicialesEstado.Where(p => p.FechaCarga.Date >= Dia && p.Procesado == 0).ToList();
                foreach (var cie in cargasInicialesEstado)
                {
                    cargaInicials = _context.CargasIniciales.Where(o => o.FechaCarga.Date >= Dia && o.CargaInicialEstado.Id == cie.Id && o.CargaInicialEstado.Procesado == 0 && (o.Estado == "Ganada" || o.Estado == "Aprobado")).Include(c => c.CargaInicialEstado).ToList();
                    foreach (var ci in cargaInicials)
                    {
                        try
                        {
                            var existeCarga = _context.Pensionado.Where(o => o.Folio == ci.Folio && o.RutCliente == ci.RutPensionado + "-" + ci.DvPensionado).FirstOrDefault();
                            if (existeCarga == null)
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
                                _context.Pensionado.Add(pensionado);
                                _context.SaveChanges();
                                procesados++;
                                output = new Output("Datos Cargados Con Exito", "00", "CargaPensionado", ci.RutPensionado + "-" + ci.DvPensionado, "RutRut");
                            }
                        }
                        catch (Exception ex)
                        {
                            output = new Output("Error Al Cargar Datos :" + ex, "10", "CargaPensionado", ci.RutPensionado + "-" + ci.DvPensionado, "RutRut");
                        }
                        if (output.Mensaje != null)
                        {
                            RegistraLog(output);
                        }
                    }
                    cie.Procesado = 1;
                    _context.CargasInicialesEstado.Update(cie);
                    _context.SaveChanges();
                    
                }

                if (procesados == 0)
                {
                    output = new Output("Sin Datos a Procesar", "01", "CargaPensionado", "FinProceso", "FinProceso");
                }
                else 
                {
                    output = new Output("Datos Cargados Con Exito", "00", "CargaPensionado", "FinProceso", "FinProceso");
                }


            }
            catch (Exception ex)
            {
                output = new Output("Error Al Cargar Datos :" + ex, "10", "CargaPensionado", "FinProceso", "FinProceso");
            }
            RegistraLog(output);

            return output;
        }
        #endregion

        #region Carga tabla Expediente
        private Output CargaExpediente( DateTime Dia) {
            Output output = new Output();
            var _context = _scope.ServiceProvider.GetRequiredService<PensionadoDbContext>();
            var procesados = 0;
            try
            {
                var pensionados = _context.Pensionado.Where(p =>p.FechaFormaliza >= Dia).ToList();

                foreach (var p in pensionados)
                {
                    try
                    {
                        var existe = _context.Expedientes.Where(e => e.Pensionado.Id == p.Id).Include(t => t.Pensionado).FirstOrDefault();
                        if (existe == null)
                        {
                            var pensionado = new Pensionado()
                            {
                                Id = p.Id,
                                FechaFormaliza = p.FechaFormaliza,
                                RutCliente = p.RutCliente,
                                NombreCliente = p.NombreCliente,
                                NumeroTicket = p.NumeroTicket,
                                TipoPensionado = p.TipoPensionado
                            };

                            var sucursal = _context.CargasIniciales.Where(c => c.Folio == p.Folio).Include(s => s.Sucursal).FirstOrDefault();
                            var expediente = new Expedientes()
                            {
                                FechaCreacion = Dia,
                                Pensionado = pensionado,
                                TipoExpediente = 0,
                                IdSucursalActividad = sucursal.Sucursal.Id.ToString()
                            };

                            _context.Expedientes.Add(expediente);
                            _context.SaveChanges();
                            output = new Output("Datos Cargados Con Exito", "00", "CargaExpediente", p.RutCliente, "Rut");
                            procesados++;
                        }
                    }
                    catch (Exception ex)
                    {
                        output = new Output("Error Al Cargar Datos :" + ex, "10", "CargaExpediente", p.RutCliente, "Rut");
                    }
                    if (output.Mensaje != null)
                    {
                        RegistraLog(output);
                    }

                }
                if (procesados == 0)
                {
                    output = new Output("Sin Datos a Procesar", "01", "CargaExpediente", "Fin Proceso", "FinProceso");
                }
                else
                {
                    output = new Output("Datos Cargados Con Exito" , "00", "CargaExpediente", "Fin Proceso", "FinProceso");
                }               
            }
            catch (Exception ex)
            {
                output = new Output("Error Al Cargar Datos :" + ex, "10", "CargaExpediente", "Fin Proceso", "FinProceso");
            }
            RegistraLog(output);
            return output;
        }
        #endregion

        #region Carga tabla Solicitud
        private Output CargaSolicitud( DateTime Dia) {
            Output output = new Output();
            var _context = _scope.ServiceProvider.GetRequiredService<PensionadoDbContext>();
            var proceso = new Procesos();
            proceso =_context.Procesos.Where(p => p.NombreInterno == "SOLICITUD_CUSTODIA_PENSIONADOS").FirstOrDefault();
            var procesados = 0;
            if (proceso != null)
            {

                var pensionado = _context.Pensionado.Where(o => o.FechaFormaliza >= Dia).AsList();

                try
                {
                    foreach (var pe in pensionado)
                    {
                        try 
                        { 
                             var existe = _context.Solicitudes.Where(p => p.NumeroTicket == pe.NumeroTicket).FirstOrDefault();

                            if (existe == null) { 
                                var solicitud = new Solicitudes()
                                {
                                    NumeroTicket = pe.NumeroTicket,
                                    Resumen = "Ingreso Automatico de pensionado",
                                    InstanciadoPor = "wfboot",
                                    Estado = "Iniciada",
                                    FechaInicio = Dia,
                                    Proceso = proceso,
                                };

                                _context.Solicitudes.Add(solicitud);
                                _context.SaveChanges();
                                output = new Output("Datos Cargados Con Exito", "00", "CargaSolicitud", pe.NumeroTicket, "NumeroTicket");
                                procesados++;
                            }
                        }
                        catch (Exception ex)
                        {
                            output = new Output("Error Al Cargar Datos :" + ex, "10", "CargaSolicitud", pe.NumeroTicket, "NumeroTicket");
                        }
                        if (output.Mensaje != null)
                        {
                            RegistraLog(output);
                        }
                    }
                }
                catch (Exception ex)
                {
                    output = new Output("Error Al Cargar Datos: " + ex, "10", "CargaSolicitud", "Fin Proceso", "FinProceso");
                }
                if (procesados == 0)
                {
                    output = new Output("Sin Datos a Procesar", "01", "CargaSolicitud", "Fin Proceso", "FinProceso");
                }
                else
                {
                    output = new Output("Datos Cargados Con Exito", "00", "CargaSolicitud", "Fin Proceso", "FinProceso");
                }

            }
            else{
                output = new Output("No Se Encunetra Proceso [SOLICITUD_CUSTODIA_PENSIONADOS] Cargado En Tabla de Procesos", "10", "CargaSolicitud", "Fin Proceso", "FinProceso");
            }
            RegistraLog(output);
            return output;
        }
        #endregion

        #region Carga tabla Documentos
        private Output CargaDocumento(DateTime Dia)
        {
            Output output = new Output();
            var _context = _scope.ServiceProvider.GetRequiredService<PensionadoDbContext>();
            var configuracionDocumentos = _context.ConfiguracionDocumentos.Where(cd => cd.Activo == 1 && cd.TipoPensionado == 1).ToList();
            var expediente = _context.Expedientes.Where(e => e.FechaCreacion == Dia).ToList();
            var procesados = 0;
            try
            {

                foreach (var e in expediente)
                {
                    foreach (var cd in configuracionDocumentos)
                    {
                        try
                        {

                            var existeDocumento = _context.Documentos.Where(d => d.ExpedienteId == e.Id && d.Codificacion == cd.Codificacion).FirstOrDefault();
                            if (existeDocumento == null)
                            {
                                var documento = new Documentos()
                                {
                                    Resumen = cd.TipoDocumento.ToString(),
                                    Codificacion = cd.Codificacion,
                                    ConfiguracionDocumento = cd,
                                    ExpedienteId = e.Id
                                };
                                _context.Documentos.Add(documento);
                                _context.SaveChanges();
                                output = new Output("Datos Cargados Con Exito", "00", "CargaDocumento", e.Id.ToString(), "ExpedienteId");
                                procesados++;
                            }
                        }
                        catch (Exception ex)
                        {
                            output = new Output("Error Al Cargar Datos: " + ex, "10", "CargaDocumento", e.Id.ToString(), "ExpedienteId");

                        }
                        if (output.Mensaje != null)
                        {
                            RegistraLog(output);
                        }
                    }
                }
                if (procesados == 0)
                {
                    output = new Output("Sin Datos a Procesar", "01", "CargaDocumento", "Fin Proceso", "FinProceso");
                }
                else
                {
                   output = new Output("Datos Cargados Con Exito" , "00", "CargaDocumento", "Fin Proceso", "FinProceso");
                }
                
            }
            catch (Exception ex)
            {
                output = new Output("Error Al Cargar Datos: " + ex, "10", "CargaDocumento", "FinProceso", "FinProceso");
            }
            RegistraLog(output);

            return output;
        }
        #endregion

        #region Carga tabla registraLog
        private void RegistraLog( Output output) {
            var _context = _scope.ServiceProvider.GetRequiredService<PensionadoDbContext>();
            try {
                var logCargaInicial = new LogCargaInicial()
                { 
                    Fecha = DateTime.Now,
                    Paso = output.Paso,
                    CampoReferencia = output.CampoReferencia,
                    Identificador = output.Identificador,
                    CodigoEstado = output.Codigo,
                    Cometario = output.Mensaje,
                };
                Debug.WriteLine("salida ["+output.Paso+"] - [" + output.Codigo + "] [" + output.Mensaje + "]");
                _context.LogCargaInicial.Add(logCargaInicial);
                _context.SaveChanges();
            }catch (Exception ex)
            {
                Debug.WriteLine("error al generar log: " + ex+ "]");
            }
        }

        #endregion

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
