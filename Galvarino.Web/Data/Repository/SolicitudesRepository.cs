using System;
using System.Collections.Generic;
using Galvarino.Web.Models.Application;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;
using Galvarino.Web.Models.Security;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Galvarino.Web.Data.Repository
{
    public class SolicitudesRepository : ISolicitudRepository
    {
        private readonly IConfiguration _conf;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SolicitudesRepository> _logger;
        public SolicitudesRepository(IConfiguration conf, ApplicationDbContext context, ILogger<SolicitudesRepository> logger)
        {
            _conf = conf;
            _context = context;
            _logger = logger;
        }

        public IEnumerable<dynamic> listarResumenInicial(string[] roles, string rut, string[] oficinas)
        {
            var theRoles = "'" + String.Join("','", roles) + "'";
            var theOffices = "'" + String.Join("','", oficinas) + "'";
           
            var respuesta = new List<dynamic>();
            using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
            {
                
                string sql = @"
                    select 
                            et.Nombre tarea,
			                count(*) total	

                    from [" + _conf.GetValue<string>("schema") + @"].Tareas ta
                    inner join [" + _conf.GetValue<string>("schema") + @"].[Etapas] et on ta.EtapaId = et.Id
                    inner join [" + _conf.GetValue<string>("schema") + @"].[Solicitudes] sl on ta.SolicitudId = sl.Id
                    inner join [" + _conf.GetValue<string>("schema") + @"].[Creditos] cr on sl.NumeroTicket = cr.NumeroTicket
                    inner join [" + _conf.GetValue<string>("schema") + @"].[ExpedientesCreditos] ex on cr.Id = ex.CreditoId
                   
                    where ta.Estado = 'Activada'
                    and ((ta.AsignadoA in (" + theRoles + @") or ta.AsignadoA = '" + rut + @"')
                            and ((ta.UnidadNegocioAsignada in (" + theOffices + @")) or ta.UnidadNegocioAsignada = null)
                        )
                    GROUP BY et.Nombre
                    ";

                respuesta = con.Query<dynamic>(sql).AsList();
            }


           
            return respuesta;
        }

        public IEnumerable<SolicitudResult> listarSolicitudes(string[] roles, string rut, string[] oficinas, string[] etapas, string order = null, string fechaConsulta="")
        {
            var theRoles = "'" + String.Join("','", roles) + "'";
            var theOffices = "'" + String.Join("','", oficinas) + "'";
            var theSteps = "'" + String.Join("','", etapas) + "'";
            var respuesta = new List<SolicitudResult>();
            var sqlTrozoFechaConsulta = "";


            if(fechaConsulta != ""){
                sqlTrozoFechaConsulta = "and convert(date, cr.FechaDesembolso) = convert(date, '"+fechaConsulta +"')";
            }


             using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
             {
                 string sql = @"
                    select 
                            cr.FolioCredito,
                            cr.RutCliente,
                            cr.TipoCredito,
                            cr.FechaDesembolso,
                            pkn.CodigoSeguimiento seguimientoNotaria,
                            pkn.FechaEnvio fechaEnvioNotaria,
                            vv.CodigoSeguimiento seguimientoValija,
                            vv.FechaEnvio fechaEnvioValija,
                            vre.Valor reparo,
                            vrn.Valor reparoNotaria,
                            vdf.Valor documentosFaltantes


                    from [" + _conf.GetValue<string>("schema") + @"].[Tareas] ta
                    inner join [" + _conf.GetValue<string>("schema") + @"].[Etapas] et on ta.EtapaId = et.Id
                    inner join [" + _conf.GetValue<string>("schema") + @"].[Solicitudes] sl on ta.SolicitudId = sl.Id
                    inner join [" + _conf.GetValue<string>("schema") + @"].[Creditos] cr on sl.NumeroTicket = cr.NumeroTicket
                    inner join [" + _conf.GetValue<string>("schema") + @"].[ExpedientesCreditos] ex on cr.Id = ex.CreditoId
                    left join [" + _conf.GetValue<string>("schema") + @"].[PacksNotarias] pkn on ex.PackNotariaId = pkn.Id
                    left join [" + _conf.GetValue<string>("schema") + @"].[ValijasValoradas] vv on ex.ValijaValoradaId = vv.Id
                    left join [" + _conf.GetValue<string>("schema") + @"].[Variables] vre on sl.NumeroTicket = vre.NumeroTicket and vre.Clave = 'CODIGO_MOTIVO_DEVOLUCION_A_SUCURSAL'
                    left join [" + _conf.GetValue<string>("schema") + @"].[Variables] vrn on sl.NumeroTicket = vrn.NumeroTicket and vrn.Clave = 'REPARO_REVISION_DOCUMENTO_LEGALIZADO'
                    left join [" + _conf.GetValue<string>("schema") + @"].[Variables] vdf on sl.NumeroTicket = vdf.NumeroTicket and vdf.Clave = 'COLECCION_DOCUMENTOS_FALTANTES'

                    where ta.Estado = 'Activada'
                    and ((ta.AsignadoA in ("+ theRoles +@") or ta.AsignadoA = '" + rut + @"')
                            and ((ta.UnidadNegocioAsignada in (" + theOffices + @") or ta.UnidadNegocioAsignada is null))
                            and (et.NombreInterno in (" + theSteps + @"))
                        )
                    "+ sqlTrozoFechaConsulta + @"
                    order by " + (order==null ? "cr.FechaDesembolso" : order);
                 _logger.LogDebug(sql);
                respuesta = con.Query<SolicitudResult>(sql).AsList();
             }


             respuesta.ForEach(s => {
                 s.Documentos = _context.Documentos
                                .Include(d => d.ExpedienteCredito)
                                .ThenInclude(f => f.Credito)
                                .Where(d => d.ExpedienteCredito.Credito.FolioCredito == s.FolioCredito);
             });

            return respuesta;
         }

        public IEnumerable<dynamic> listarValijasEnviadas(string marcaAvance)
        {
            var respuesta = new List<dynamic>();
            using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
            {
                string sql = @"
                    select 	vav.CodigoSeguimiento,
                            vav.FechaEnvio,
                            ofc.Nombre NombreOficina,
                            count(exp.Id) NroExpedientes
                    from [" + _conf.GetValue<string>("schema") + @"].[ValijasValoradas] vav
                    inner join [" + _conf.GetValue<string>("schema") + @"].[Oficinas] ofc on vav.OficinaId = ofc.Id
                    inner join [" + _conf.GetValue<string>("schema") + @"].[ExpedientesCreditos] exp on vav.Id = exp.ValijaValoradaId
                    where vav.MarcaAvance = '" + marcaAvance + @"'
                    GROUP BY vav.CodigoSeguimiento,
                                    vav.FechaEnvio,
                                    ofc.Nombre	    
                ";
                _logger.LogDebug(sql);
                respuesta = con.Query<dynamic>(sql).AsList();
            }
            return respuesta;
        }
    }


    public class SolicitudResult
    {
        public string FolioCredito { get; set; }
        public string RutCliente { get; set; }
        public TipoCredito TipoCredito { get; set; }
        public IEnumerable<Documento> Documentos { get; set;}
        public DateTime FechaDesembolso { get; set; }
        public string seguimientoNotaria { get; set; }
        public DateTime fechaEnvioNotaria { get; set; }
        public string seguimientoValija { get; set; }
        public DateTime fechaEnvioValija { get; set; }
        public string reparo { get; set; }
        public string reparoNotaria { get; set; }
        public string documentosFaltantes { get; set; }
                   
    }
    
}