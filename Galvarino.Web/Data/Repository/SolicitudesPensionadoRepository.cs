using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Galvarino.Web.Models.Application;
using Galvarino.Web.Models.Application.Pensionado;
using Galvarino.Web.Models.Exception;
using Galvarino.Web.Models.Helper;
using Galvarino.Web.Models.Helper.Pensionado;
using Galvarino.Web.Models.Security;
using Galvarino.Web.Services.Workflow.Assignment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using Remotion.Linq.Clauses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Galvarino.Web.Data.Repository
{

    public class SolicitudesPensionadoRepository : ISolicitudPensionadoRepository
    {
        private readonly PensionadoDbContext _context;
        private readonly ApplicationDbContext _appContext;
        private object not;

        public SolicitudesPensionadoRepository(PensionadoDbContext context, ApplicationDbContext appContex)
        {
            _context = context;
            _appContext = appContex;
        }

        public string AvanzarRango(string nombreInternoProceso, string nombreInternoEtapa, IEnumerable<string> numeroTicket, string identificacionUsuario)
        {
            // throw new NotImplementedException();

            foreach (var item in numeroTicket)
            {

                try
                {

                    Procesos proceso = _context.Procesos.FirstOrDefault(p => p.NombreInterno == nombreInternoProceso);
                    Solicitudes solicitud = _context.Solicitudes.Where(c => c.NumeroTicket == item).FirstOrDefault();
                    Tareas tareaActual = _context.Tareas
                                   .Include(s => s.Solicitudes)
                                   .Include(e => e.Etapas)
                                   .Where(t => t.Solicitudes.NumeroTicket == solicitud.NumeroTicket
                                            && t.Etapas.NombreInterno == nombreInternoEtapa
                                            && t.Estado == "Activa")
                                   .FirstOrDefault();

                    var etapa = _context.Etapas.Where(e => e.NombreInterno == nombreInternoEtapa).FirstOrDefault();

                    if (tareaActual == null)
                    {
                        throw new TareaNoEnEtapaException($"Tarea no en etapa, proceso/etapa/ticket:[{nombreInternoProceso}][{nombreInternoEtapa}][{numeroTicket}]");
                    }


                    tareaActual.EjecutadoPor = identificacionUsuario;
                    tareaActual.FechaTerminoFinal = DateTime.Now;
                    tareaActual.Estado = "Finalizada";

                    var fin = _context.Etapas.Where(e => e.NombreInterno == "FIN").FirstOrDefault();
                    if (tareaActual.Etapas.TipoEtapa == fin.TipoEtapa)
                    {
                        tareaActual.Estado = "Finalizada";
                        solicitud.FechaTermino = DateTime.Now;
                        _context.Solicitudes.Update(solicitud);
                    }

                    var transiciones = _context.Transiciones.Where(d => d.EtapaActaualId == etapa.Id).ToList();


                    foreach (var transicion in transiciones)
                    {

                        var newTransicion = from t in _context.Transiciones
                                            join ei in _context.Etapas on t.EtapaActaualId equals ei.Id
                                            join ed in _context.Etapas on t.EtapaDestinoId equals ed.Id
                                            where t.EtapaDestinoId == transicion.EtapaDestinoId && t.EtapaActaualId == transicion.EtapaActaualId
                                            select new { t.EtapaActaualId, NombreActual = ei.NombreInterno, t.EtapaDestinoId, NombreDestino = ed.NombreInterno, ed.ValorUsuarioAsignado };

                        var nextTransicion = newTransicion.FirstOrDefault();

                        var newEtapa = _context.Etapas.Where(d => d.Id == nextTransicion.EtapaDestinoId).FirstOrDefault();

                        Console.WriteLine(numeroTicket);
                        //Primero obtengo el proceso
                        //Segundo Obtengo la etapa
                        //Tercero obtengo la solicitud
                        string usuarioAsignado = etapa.ValorUsuarioAsignado;

                        //if (etapa.TipoUsuarioAsignado == "Rol")
                        //    {
                        //        usuarioAsignado = new MotorAsignacion(_context, etapa.ValorUsuarioAsignado, numeroTicket).GetResult();
                        //    }
                        //    else if (etapa.TipoUsuarioAsignado == TipoUsuarioAsignado.Variable)
                        //    {
                        //        usuarioAsignado = GetVariableValue(etapa.ValorUsuarioAsignado, numeroTicket);
                        //    }


                        Tareas tarea = new Tareas
                        {
                            Solicitudes = solicitud,
                            Etapas = newEtapa,
                            AsignadoA = nextTransicion.ValorUsuarioAsignado,
                            EjecutadoPor = nextTransicion.ValorUsuarioAsignado,
                            Estado = "Activa",
                            FechaInicio = DateTime.Now
                        };



                        _context.Tareas.Add(tarea);


                        //if (etapa.TipoUsuarioAsignado == TipoUsuarioAsignado.Boot)
                        //{
                        //    CompletarTareaMulti(nombreInternoProceso, nombreInternoEtapa, numeroTicket, identificacionUsuario);
                        //}

                    }


                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
            _context.SaveChanges();
            return ("OK");



        }


        public IEnumerable<SolicitudPensionados> ListarSolicitudes(string etapaIn = "")
        {

            var laSalida = from tarea in _context.Tareas
                           join solicitud in _context.Solicitudes on tarea.Solicitudes.Id equals solicitud.Id
                           join pensionado in _context.Pensionado on solicitud.NumeroTicket equals pensionado.NumeroTicket
                           join etapas in _context.Etapas on tarea.Etapas.Id equals etapas.Id
                           join tipo in _context.Tipo on pensionado.TipoPensionado equals tipo.Id
                           orderby solicitud.FechaInicio ascending, pensionado.RutCliente ascending
                           where etapas.NombreInterno == etapaIn && tarea.Estado == "Activa"
                           select new
                           {
                               pensionado.Folio,
                               pensionado.RutCliente,
                               pensionado.NombreCliente,
                               tipo.TipoDescripcion,
                               tipo.Motivo,
                               pensionado.Id,
                               solicitud.FechaInicio
                           };

            var respuesta = new List<SolicitudPensionados>();

            foreach (var t in laSalida)
            {
                SolicitudPensionados solicitudPensionados = new SolicitudPensionados();

                var docs = from d in _context.Documentos
                           join cd in _context.ConfiguracionDocumentos on d.ConfiguracionDocumento.Id equals cd.Id
                           join exp in _context.Expedientes on d.ExpedienteId equals exp.Id
                           where exp.Pensionado.Id == t.Id
                           select new { cd.Codificacion, cd.NombreDocumento }
                           ;

                var docu = new Docs[docs.Count()];
                int cont = 0;
                foreach (var d in docs)
                {
                    Docs documento = new Docs
                    {
                        Codificacion = d.Codificacion,
                        NombreDocumento = d.NombreDocumento
                    };
                    docu[cont] = documento;
                    cont++;
                }

                solicitudPensionados.Folio = t.Folio;
                solicitudPensionados.RutCliente = t.RutCliente;
                solicitudPensionados.NombreCliente = t.NombreCliente;
                solicitudPensionados.Tipo = t.TipoDescripcion;
                solicitudPensionados.Motivo = t.Motivo;
                solicitudPensionados.Documento = docu;
                solicitudPensionados.Fecha = t.FechaInicio.ToString("dd-MM-yyyy");

                respuesta.Add(solicitudPensionados); ;
            }

            return (respuesta);
        }

        public object listarSolicitudes(string etapaIn)
        {
            throw new NotImplementedException();
        }


        public ExpendientesBuscado ObtenerExpedientes(string folio, string etapaSolicitud = "", string oficinaUsuario = "")
        {
            var expedientes = _context.Expedientes.Include(p => p.Pensionado).Where(p => p.Pensionado.Folio == folio).FirstOrDefault();
            //var documentos = _context.Documentos.Include(cd => cd.ConfiguracionDocumento).Where(d => d.ExpedienteId == expedientes.Id);
            var expediente = new ExpendientesBuscado();

            var documentos = from d in _context.Documentos
                             join cd in _context.ConfiguracionDocumentos on d.ConfiguracionDocumento.Id equals cd.Id
                             where d.ExpedienteId == expedientes.Id
                             select new { cd.Codificacion, cd.NombreDocumento };

            var docu = new Docs[documentos.Count()];
            int cont = 0;
            foreach (var d in documentos)
            {
                Docs documento = new Docs
                {
                    Codificacion = d.Codificacion.ToString(),
                    NombreDocumento = d.NombreDocumento
                };
                docu[cont] = documento;
                cont++;
            }


            var tareas = from t in _context.Tareas
                         join e in _context.Etapas on t.Etapas.Id equals e.Id
                         join s in _context.Solicitudes on t.Solicitudes.Id equals s.Id
                         where s.NumeroTicket == expedientes.Pensionado.NumeroTicket && t.Estado == "Activa"
                         select new { e.Secuencia, e.NombreInterno };

            if (expedientes != null)
            {
                if (tareas.FirstOrDefault().NombreInterno == etapaSolicitud)
                {
                    expediente.Id = expedientes.Id;
                    expediente.Pensionado = expedientes.Pensionado;
                    expediente.CajaValoradaId = expedientes.CajaValoradaId;
                    expediente.FechaCreacion = expediente.FechaCreacion;
                    expediente.TipoExpediente = expedientes.TipoExpediente;
                    expediente.ValijaValoradaId = expediente.ValijaValoradaId;
                    expediente.ValijaOficinaId = expediente.ValijaOficinaId;
                    expediente.IdSucursalActividad = expediente.IdSucursalActividad;
                    expediente.Documentos = docu;
                    expediente.Comentario = "Este Expediente encontrado: <strong>" + tareas.FirstOrDefault().NombreInterno + "</strong>";
                }
                else
                {
                    expediente.Comentario = "Este Expediente esta en la siguiente tarea: <strong>" + tareas.FirstOrDefault().NombreInterno + "</strong>";
                }

            }
            else
            {
                expediente.Comentario = "Carga del Expediente en la siguiente Oficina: ";
            }

            return (expediente);
        }


        public ValijasValoradas DespachoOfPartes(string nombreInternoProceso, string nombreInternoEtapa, IEnumerable<ColeccionDespachoPartes> despachos, string identificacionUsuario,string user)
        {
            
            List<Expedientes> expedientesModificados = new List<Expedientes>();
            List<string> ticketsAvanzar = new List<string>();


            var oficinaEnvio = _appContext.Oficinas.FirstOrDefault(d => d.Codificacion == identificacionUsuario);
            DateTime now = DateTime.Now;
            var codSeg = now.Ticks.ToString() + "X" + oficinaEnvio.Codificacion;
            HomologacionOficinas homologacionOficina = _context.HomologacionOficinas.Where(h => h.Codificacion == oficinaEnvio.Codificacion).FirstOrDefault();

            var valijaEnvio = new ValijasValoradas
            {
                FechaEnvio = DateTime.Now,
                HomologacionOficinas = homologacionOficina,
                CodigoSeguimiento = codSeg,
                MarcaAvance = "OF_PARTES"
            };
            _context.ValijasValoradas.Add(valijaEnvio);
            _context.SaveChanges();

            var idValija = from v in _context.ValijasValoradas
                           where v.CodigoSeguimiento == codSeg
                           select new { v.Id};
           
            foreach (var item in despachos)
            {
                var pensionado = _context.Pensionado.FirstOrDefault(x => x.Folio == item.Folio);
                var expediente = _context.Expedientes.FirstOrDefault(ex => ex.Pensionado.Id == pensionado.Id );
                expediente.ValijaValoradaId = idValija.First().Id;
                expedientesModificados.Add(expediente);
                ticketsAvanzar.Add(expediente.Pensionado.NumeroTicket);
                
            }
            _context.Expedientes.UpdateRange(expedientesModificados); 
            _context.SaveChanges();

            AvanzarRango(nombreInternoProceso, nombreInternoEtapa, ticketsAvanzar, user);


            return (valijaEnvio);
        }



    }

    public class SolicitudPensionados
    {
        public string Folio { get; set; }
        public string RutCliente { get; set; }
        public IEnumerable<Docs> Documento { get; set; }
        public string NombreCliente { get; set; }
        public string Tipo { get; set; }
        public string Motivo { get; set; }
        public string Fecha { get; set; }
    }

    public class Docs
    {
        public string Codificacion { get; set; }
        public string NombreDocumento { get; set; }
    }

    public class ExpendientesBuscado
    {
        public int Id { get; set; }
        public DateTime FechaCreacion { get; set; }
        public Pensionado Pensionado { get; set; }
        public int TipoExpediente { get; set; }
        public int ValijaValoradaId { get; set; }
        public int CajaValoradaId { get; set; }
        public int ValijaOficinaId { get; set; }
        public string IdSucursalActividad { get; set; }
        public IEnumerable<Docs> Documentos { get; set; }
        public string Comentario { get; set; }

    }
}