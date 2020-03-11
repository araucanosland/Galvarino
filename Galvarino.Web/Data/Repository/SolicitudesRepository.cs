using Dapper;
using Galvarino.Web.Models.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

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






        public IEnumerable<dynamic> listarSegurosPorValijas(string skp, string rut)
        {
            var respuesta = new List<dynamic>();
            using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
            {
                string sql = " select pvv.foliocredito folio, count(pvv.foliocredito)pistoleados," +
                " (select count(*) from Documentos where ExpedienteCreditoId = ec.Id" +
                " and Codificacion in ('09', '10'))total" +
                " from  PasosValijasValoradas pvv" +
                ", Creditos c" +
                " ,ExpedientesCreditos ec" +
                " where pvv.CodigoCajaValorada = '" + skp + "'" +
                " and c.FolioCredito = pvv.FolioCredito" +
                " and ec.CreditoId = c.Id" +
                " and pvv.Usuario ='" + rut + "'" +
                " group by pvv.FolioCredito," +
                " pvv.CodigoCajaValorada," +
                " pvv.Usuario," +
                " c.TipoCredito,ec.Id";
                respuesta = con.Query<dynamic>(sql).AsList();
            }
            return respuesta;
        }


        public IEnumerable<SolicitudResult> listarSolicitudesSc(string[] roles, string rut, string[] oficinas, string[] etapas, string order = null, string fechaConsulta = "", string sucursal = "")
        {

            try
            {
                var theRoles = "'" + String.Join("','", roles) + "'";
                var theOffices = "'" + String.Join("','", oficinas) + "'";
                var theSteps = "'" + String.Join("','", etapas) + "'";
                var respuesta = new List<SolicitudResult>();



                using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
                {

                    string sql = "SELECT  distinct  cr.FolioCredito, " +
                       " cr.RutCliente, " +
                       " cr.TipoCredito, " +
                       " cr.FechaDesembolso, " +
                       " pkn.CodigoSeguimiento seguimientoNotaria, " +
                       " pkn.FechaEnvio fechaEnvioNotaria, " +
                       //" vv.CodigoSeguimiento seguimientoValija," +
                       //" vv.FechaEnvio fechaEnvioValija," +
                       " et.NombreInterno, " +
                       " ta.UnidadNegocioAsignada," +
                       " ci.CodigoOficinaIngreso, " +
                       " ci.CodigoOficinaPago, " +
                       " o.Nombre descipcionOficina," +
                       " opag.Nombre descipcionOficinapagadora,";
                    if (theSteps == "'ANALISIS_MESA_CONTROL_SET_COMPLEMETARIOS'")
                    {
                        sql = sql + " variab.Valor reparo";
                    }
                    else
                    {
                        sql = sql + " '' reparo" ;

                    }

                    sql = sql + " FROM [" + _conf.GetValue<string>("schema") + @"].[ExpedientesComplementarios]  ec" +
                    " INNER JOIN [" + _conf.GetValue<string>("schema") + @"].[Creditos] cr ON cr.id = ec.creditoid" +
                    " INNER JOIN[" + _conf.GetValue<string>("schema") + @"].[Solicitudes] sl ON sl.NumeroTicket=ec.NumeroTicket" +
                    " INNER join[" + _conf.GetValue<string>("schema") + @"].[Tareas] ta on ta.solicitudid=sl.Id" +
                    " INNER JOIN[" + _conf.GetValue<string>("schema") + @"].[Etapas] et ON et.Id=ta.EtapaId" +
                    " INNER JOIN [" + _conf.GetValue<string>("schema") + @"].[ExpedientesCreditos] ex ON ex.CreditoId = ec.creditoid" +
                    " LEFT JOIN [" + _conf.GetValue<string>("schema") + @"].[PacksNotarias] pkn ON ex.PackNotariaId = pkn.Id" +
                    " LEFT JOIN [" + _conf.GetValue<string>("schema") + @"].[ValijasValoradas] vv ON ex.ValijaValoradaId = vv.Id" +
                    " INNER JOIN [" + _conf.GetValue<string>("schema") + @"].CargasIniciales ci ON ci.FolioCredito = cr.FolioCredito" +
                    " INNER JOIN [" + _conf.GetValue<string>("schema") + @"].Oficinas o ON ci.CodigoOficinaIngreso = o.Codificacion" +
                    " INNER JOIN [" + _conf.GetValue<string>("schema") + @"].Oficinas opag ON ci.CodigoOficinaPago = opag.Codificacion";

                    if (theSteps == "'ANALISIS_MESA_CONTROL_SET_COMPLEMETARIOS'")
                        sql = sql + " LEFT OUTER JOIN Variables variab on variab.NumeroTicket = ec.NumeroTicket and variab.Clave = 'DEVOLUCION_A_SUCURSAL_PAGO_SET_COMPLEMENTARIOS'";


                    sql = sql + " where ta.Estado = 'Activada'" +
                    " and et.ProcesoId = 2" +
                    " and (ta.AsignadoA in (" + theRoles + ")  or ta.AsignadoA =  '" + rut + "')";

                    if (theSteps == "'DESPACHO_A_DOCUMENTOS_A_SUCURSAL'")
                    {
                        sql = sql + "  and  ci.CodigoOficinaIngreso!= ci.CodigoOficinaPago";

                    }
                    else if (theSteps == "'PREPARAR_NOMINA_SET_COMPLEMENTARIO'" || theSteps == "'DESPACHO_OFICINA_DE_PARTES_SET_COMPLEMENTARIO'" || theSteps == "'SOLUCION_REPAROS_SUCURSAL_PAGADORA'")
                    {
                        sql = sql + " and (ci.CodigoOficinaPago in (" + theOffices + "))";
                    }

                    sql = sql + " and (et.NombreInterno in (" + theSteps + "))" +
                    " and ('' = '' or pkn.NotariaEnvioId = '')    ";

                    if (fechaConsulta != "")
                    {
                        sql = sql + " and convert(varchar, cr.FechaDesembolso,112) =    CONVERT(varchar, convert(datetime, '" + fechaConsulta + "'),112)";
                    }

                    if (sucursal != "")
                    {
                        sql = sql + " AND ( ci.CodigoOficinaIngreso= '" + sucursal + "' Or '" + sucursal + "'='AA00') and ex.ValijaValoradaId is not null ";


                    }
                    sql = sql + "  order by cr.folioCredito asc";


                    var item = con.Query<SolicitudResult>(sql, commandTimeout: 400, buffered: true);
                    respuesta = item.ToList();

                    //respuesta = con.Query<SolicitudResult>(sql).AsList() ;
                }



                var salida = new List<Documento>();
                using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
                {
                    if (theSteps == "'DESPACHO_A_CUSTODIA_SET_COMPLEMENTARIO'")
                    {
                        respuesta.ForEach(s =>
                        {

                            string sql = @"
                   select  d.Id,d.Resumen,d.Codificacion,d.TipoDocumento,ExpedienteCreditoId
                   from [" + _conf.GetValue<string>("schema") + @"].ExpedientesCreditos ec
                   , [" + _conf.GetValue<string>("schema") + @"].Documentos d
                   ,[" + _conf.GetValue<string>("schema") + @"].ConfiguracionDocumentos cd
                   ,[" + _conf.GetValue<string>("schema") + @"].Creditos c
                   where d.ExpedienteCreditoId = ec.Id
                   and cd.Codificacion = d.Codificacion
                   and d.Codificacion in('10','09') 
                   and ec.CreditoId=c.Id
                   and cd.TipoExpediente=2
				   and c.FolioCredito='" + s.FolioCredito + "' order by d.codificacion asc";
                            salida = con.Query<Documento>(sql).AsList();

                            s.Documentos = salida;


                        });
                    }
                    else if (theSteps == "'DESPACHO_A_DOCUMENTOS_A_SUCURSAL'" || theSteps == "'RECEPCION_DOCUMENTOS_OF_EVALUADORA'")
                    {

                        respuesta.ForEach(s =>
                        {

                            string sql = @"
                   select d.Id,d.Resumen,d.Codificacion,d.TipoDocumento,ExpedienteCreditoId
                   from [" + _conf.GetValue<string>("schema") + @"].ExpedientesCreditos ec
                   , [" + _conf.GetValue<string>("schema") + @"].Documentos d
                   ,[" + _conf.GetValue<string>("schema") + @"].ConfiguracionDocumentos cd
                   ,[" + _conf.GetValue<string>("schema") + @"].Creditos c
                   where d.ExpedienteCreditoId = ec.Id
                   and cd.Codificacion = d.Codificacion
                   and d.Codificacion not in('10','09') 
                   and ec.CreditoId=c.Id
                   and cd.TipoExpediente=2
				   and c.FolioCredito='" + s.FolioCredito + "' order by d.codificacion asc";
                            salida = con.Query<Documento>(sql).AsList();

                            s.Documentos = salida;


                        });


                    }
                    else
                    {

                        respuesta.ForEach(s =>
                        {

                            string sql = @"
                   select d.Id,d.Resumen,d.Codificacion,d.TipoDocumento,ExpedienteCreditoId
                   from [" + _conf.GetValue<string>("schema") + @"].ExpedientesCreditos ec
                   , [" + _conf.GetValue<string>("schema") + @"].Documentos d
                   ,[" + _conf.GetValue<string>("schema") + @"].ConfiguracionDocumentos cd
                   ,[" + _conf.GetValue<string>("schema") + @"].Creditos c
                   where d.ExpedienteCreditoId = ec.Id
                   and cd.Codificacion = d.Codificacion
                   and ec.CreditoId=c.Id
                   and cd.TipoExpediente=2
				   and c.FolioCredito='" + s.FolioCredito + "' order by d.codificacion asc";
                            salida = con.Query<Documento>(sql).AsList();

                            s.Documentos = salida;


                        });


                    }



                }

                return respuesta;
            }
            catch (Exception ex)
            {

                throw;
            }

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
                    and  ex.tipoExpediente=0
                    and  sl.procesoid=1
                    and ((ta.AsignadoA in (" + theRoles + @") or ta.AsignadoA = '" + rut + @"')
                            and ((ta.UnidadNegocioAsignada in (" + theOffices + @")) or ta.UnidadNegocioAsignada = null)
                        )
                    GROUP BY et.Nombre
                    ";

                respuesta = con.Query<dynamic>(sql).AsList();
            }



            return respuesta;
        }

        public IEnumerable<dynamic> listarSucursalespoEtapa()
        {
            var respuesta = new List<dynamic>();
            using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
            {
                string sql = "  select distinct  o.Codificacion, o.Nombre from Etapas e" +
                       " inner join [" + _conf.GetValue<string>("schema") + @"].Tareas t on t.EtapaId = e.id" +
                       " inner join [" + _conf.GetValue<string>("schema") + @"].Solicitudes s on s.id = t.SolicitudId" +
                       " inner join [" + _conf.GetValue<string>("schema") + @"].ExpedientesComplementarios ec on ec.NumeroTicket = s.NumeroTicket" +
                       " inner join [" + _conf.GetValue<string>("schema") + @"].CargasIniciales ci on ci.FolioCredito = ec.FolioCredito" +
                       " inner join [" + _conf.GetValue<string>("schema") + @"].Oficinas o on o.Codificacion = ci.CodigoOficinaIngreso" +
                       " where t.Estado = 'Activada'" +
                       " and ci.CodigoOficinaIngreso != ci.CodigoOficinaPago" +
                       " AND e.NombreInterno = 'DESPACHO_A_DOCUMENTOS_A_SUCURSAL'";
                respuesta = con.Query<dynamic>(sql).AsList();
            }
            return respuesta;
        }


        public int listarDocumentosReparosSc(string[] roles, string rut, string[] oficinas, string[] etapas)
        {

            var theRoles = "'" + String.Join("','", roles) + "'";
            var theOffices = "'" + String.Join("','", oficinas) + "'";
            var theSteps = "'" + String.Join("','", etapas) + "'";

            //var respuesta = new List<dynamic>();
            using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
            {
                string sql = @" select count(*) from [" + _conf.GetValue<string>("schema") + @"].Tareas t
                         ,[" + _conf.GetValue<string>("schema") + @"].Solicitudes s
                         ,[" + _conf.GetValue<string>("schema") + @"].Etapas e
                         ,[" + _conf.GetValue<string>("schema") + @"].ExpedientesComplementarios ec
                         ,[" + _conf.GetValue<string>("schema") + @"].CargasIniciales ci
                        where t.SolicitudId = s.Id
                        and t.Estado = 'Activada'
                        and t.EtapaId = e.Id
                        and ec.NumeroTicket = s.NumeroTicket
                        and ci.FolioCredito = ec.FolioCredito
                        and(ci.CodigoOficinaPago in (" + theOffices + @"))
                        and (t.AsignadoA in (" + theRoles + @") or t.AsignadoA = '" + rut + @"')
                        and (e.NombreInterno in (" + theSteps + "))";

                var result = con.ExecuteScalar<int>(sql);
                return result;
            }
           
        }




        public IEnumerable<SolicitudResult> listarSolicitudes(string[] roles, string rut, string[] oficinas, string[] etapas, string order = null, string fechaConsulta = "", string notaria = "")
        {
            var theRoles = "'" + String.Join("','", roles) + "'";
            var theOffices = "'" + String.Join("','", oficinas) + "'";
            var theSteps = "'" + String.Join("','", etapas) + "'";
            var respuesta = new List<SolicitudResult>();
            var sqlTrozoFechaConsulta = "";


            if (fechaConsulta != "")
            {
                sqlTrozoFechaConsulta = "and convert(date, cr.FechaDesembolso) = convert(date, '" + fechaConsulta + "')";
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
                    and ((ta.AsignadoA in (" + theRoles + @") or ta.AsignadoA = '" + rut + @"')
                            and ((ta.UnidadNegocioAsignada in (" + theOffices + @") or ta.UnidadNegocioAsignada is null))
                            and (et.NombreInterno in (" + theSteps + @"))
                        )
                    and sl.ProcesoId=1
                    and ex.TipoExpediente=0
                    and ('" + notaria + @"' = '' or pkn.NotariaEnvioId = '" + notaria + @"')    
                    " + sqlTrozoFechaConsulta + @"
                    order by " + (order == null ? "cr.FechaDesembolso" : order);
                _logger.LogDebug(sql);
                //respuesta = con.Query<SolicitudResult>(sql).AsList();
                var item = con.Query<SolicitudResult>(sql, commandTimeout: 400, buffered: true);
                respuesta = item.ToList();
            }


            respuesta.ForEach(s =>
            {
                s.Documentos = _context.Documentos
                               .Include(d => d.ExpedienteCredito)
                               .ThenInclude(f => f.Credito)
                               .Where(d => d.ExpedienteCredito.Credito.FolioCredito == s.FolioCredito && d.ExpedienteCredito.TipoExpediente == 0);
            });

            return respuesta;
        }

        public IEnumerable<SolicitudResult> listarSolicitudesNominaEspecial(string[] roles, string rut, string[] oficinas, string[] etapas, string order = null, string fechaConsulta = "", string notaria = "")
        {
            var theRoles = "'" + String.Join("','", roles) + "'";
            var theOffices = "'" + String.Join("','", oficinas) + "'";
            var theSteps = "'" + String.Join("','", etapas) + "'";
            var respuesta = new List<SolicitudResult>();
            var sqlTrozoFechaConsulta = "";


            if (fechaConsulta != "")
            {
                sqlTrozoFechaConsulta = "and convert(date, cr.FechaDesembolso) = convert(date, '" + fechaConsulta + "')";
            }


            using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
            {
                string sql = @"
                    select  distinct 
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
                            vdf.Valor documentosFaltantes,
                            pvv.CodigoCajaValorada codigoCajaValorada

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
                    left outer join [" + _conf.GetValue<string>("schema") + @"].[PasosValijasValoradas] pvv on pvv.FolioCredito=cr.FolioCredito
					left outer join [" + _conf.GetValue<string>("schema") + @"].[CajasValoradas] cv on cv.CodigoSeguimiento=pvv.CodigoCajaValorada
                    where ta.Estado = 'Activada'
                    and ((ta.AsignadoA in (" + theRoles + @") or ta.AsignadoA = '" + rut + @"')
                            and ((ta.UnidadNegocioAsignada in (" + theOffices + @") or ta.UnidadNegocioAsignada is null))
                            and (et.NombreInterno in (" + theSteps + @"))
                        )
                    and ('" + notaria + @"' = '' or pkn.NotariaEnvioId = '" + notaria + @"') ";
                _logger.LogDebug(sql);
                respuesta = con.Query<SolicitudResult>(sql).AsList();
            }


            respuesta.ForEach(s =>
            {
                s.Documentos = _context.Documentos
                               .Include(d => d.ExpedienteCredito)
                               .ThenInclude(f => f.Credito)
                               .Where(d => d.ExpedienteCredito.Credito.FolioCredito == s.FolioCredito && d.ExpedienteCredito.TipoExpediente == 0);
            });

            return respuesta;
        }

        public IEnumerable<dynamic> listarOficinas(string EsRM)
        {
            var respuestaOficinas = new List<dynamic>();
            using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
            {
                string sql = @"select Id,Nombre from [" + _conf.GetValue<string>("schema") + @"].oficinas order by 2";
                respuestaOficinas = con.Query<dynamic>(sql).AsList();

            }

            return respuestaOficinas;
        }


        public IEnumerable<ValijaValorada> listarNominasGeneradasOfPagoSc()
        {
            var respuesta = new List<ValijaValorada>();
            var expdientes = new List<ExpedienteCredito>();
            using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
            {
                string sql = @"select distinct vv.* from[" + _conf.GetValue<string>("schema") + @"]. ExpedientesComplementarios ec
                             ,[" + _conf.GetValue<string>("schema") + @"].ValijasValoradas vv
                             where ec.CodigoSeguimiento is not null
                             and vv.CodigoSeguimiento=ec.CodigoSeguimiento";
                respuesta = con.Query<ValijaValorada>(sql).AsList();
                respuesta.ForEach(s =>
                {

                    string sqlExpediente = @"
                            select ec.* from [" + _conf.GetValue<string>("schema") + @"].ValijasValoradas vv
                            ,[" + _conf.GetValue<string>("schema") + @"].ExpedientesCreditos ec
                            ,[" + _conf.GetValue<string>("schema") + @"].ExpedientesComplementarios ecom
                            where vv.CodigoSeguimiento=ecom.CodigoSeguimiento
                            and ec.TipoExpediente=2
                            and ec.CreditoId=ecom.CreditoId
                            and vv.CodigoSeguimiento='" + s.CodigoSeguimiento + "'";

                    expdientes = con.Query<ExpedienteCredito>(sqlExpediente).AsList();

                    s.Expedientes = expdientes;


                });

                return respuesta;
            }

        }



        public IEnumerable<ValijaValorada> listarNominasGeneradasSc()
        {
            var respuesta = new List<ValijaValorada>();
            var expdientes = new List<ExpedienteCredito>();
            using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
            {
                string sql = @"select distinct  vv.* from ValijasValoradas vv
                            ,ExpedientesCreditos ec
                            where vv.Id=ec.ValijaValoradaId
                            and ec.TipoExpediente=2";
                respuesta = con.Query<ValijaValorada>(sql).AsList();
                respuesta.ForEach(s =>
                {

                    string sqlExpediente = @"
                              select ec.* from ValijasValoradas vv
                            ,ExpedientesCreditos ec
                            ,ExpedientesComplementarios ecom
                            where vv.Id=ec.ValijaValoradaId
                            and ec.CreditoId=ecom.CreditoId
                            and vv.CodigoSeguimiento='" + s.CodigoSeguimiento + "'";
                    expdientes = con.Query<ExpedienteCredito>(sqlExpediente).AsList();

                    s.Expedientes = expdientes;


                });

                return respuesta;
            }

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
                    and exp.tipoExpediente=0 
                    GROUP BY vav.CodigoSeguimiento,
                                    vav.FechaEnvio,
                                    ofc.Nombre";
                _logger.LogDebug(sql);
                respuesta = con.Query<dynamic>(sql).AsList();
            }
            return respuesta;
        }


        public IEnumerable<dynamic> listarValijasEnviadasOficinaIngresoSc(string marcaAvance, string[] roles, string rut, string[] oficinas, string[] etapas)
        {
            var respuesta = new List<dynamic>();
            var theRoles = "'" + String.Join("','", roles) + "'";
            var theOffices = "'" + String.Join("','", oficinas) + "'";
            var theSteps = "'" + String.Join("','", etapas) + "'";
            var RolVisualizador = "";
            foreach (var item in roles)
            {
                if ("Mantenedor de Sistema" == item)
                {
                    RolVisualizador = item;
                }
            }
            using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
            {
                string sql =

                   @"  select      vav.CodigoSeguimiento,
                        vav.FechaEnvio,
                         ofc.Nombre NombreOficina,
                        count(exp.Id) NroExpedientes
                        from[dbo].[ValijasValoradas]     vav
                       inner join[" + _conf.GetValue<string>("schema") + @"].[ExpedientesComplementarios] ecom on ecom.CodigoSeguimiento=vav.CodigoSeguimiento
                       inner join[" + _conf.GetValue<string>("schema") + @"].[creditos] c on c.FolioCredito=ecom.FolioCredito
                       inner join[" + _conf.GetValue<string>("schema") + @"].[ExpedientesCreditos] exp on exp.CreditoId=c.Id
                       inner join[" + _conf.GetValue<string>("schema") + @"].[CargasIniciales] ci on ci.FolioCredito=c.FolioCredito
                       inner join[" + _conf.GetValue<string>("schema") + @"].[Oficinas] ofc on ci.CodigoOficinaIngreso = ofc.Codificacion
                       inner join [" + _conf.GetValue<string>("schema") + @"].Solicitudes s on s.NumeroTicket=ecom.NumeroTicket
					   inner join [" + _conf.GetValue<string>("schema") + @"].Tareas ta on ta.SolicitudId=s.Id
                         inner join [" + _conf.GetValue<string>("schema") + @"].etapas e on e.id=ta.etapaId
                     where vav.MarcaAvance = '" + marcaAvance + @"'
                   and exp.tipoExpediente=2";


                if (theSteps == "'RECEPCION_VALIJA_DOCUMENTO_OFICINA_DESTINO'" && RolVisualizador != "Mantenedor de Sistema")// valida perfil de visualcio de oficinas de ingreso
                {
                    sql = sql + " and ci.CodigoOficinaIngreso in (" + theOffices + @") ";
                }
                if (theSteps == "'APERTURA_VALIJA_DOCUMENTOS_OFICINA_DESTINO'" && RolVisualizador != "Mantenedor de Sistema")// valida perfil de visualcio de oficinas de ingreso
                {
                    sql = sql + " and ci.CodigoOficinaIngreso in (" + theOffices + @")";
                }

                sql = sql + " and ta.Estado = 'Activada'" +
                      "  and ((ta.AsignadoA in (" + theRoles + ") or ta.AsignadoA = '" + rut + "'))" +
                      "  and e.NombreInterno=" + theSteps + "";

                sql = sql + @" GROUP BY vav.CodigoSeguimiento,
                                    vav.FechaEnvio,
                                    ofc.Nombre";


                _logger.LogDebug(sql);
                respuesta = con.Query<dynamic>(sql).AsList();
            }
            return respuesta;
        }


        public void ReasginarEstadoTarea(string folioCredito, int nuevaEtapa)
        {
            using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
            {
                var creditos = _context.Creditos.Where(a => a.FolioCredito == folioCredito).FirstOrDefault();
                string actualizar = @" declare @p_id_solicitud int" +
                     " declare @p_unidadNegocio varchar(5)" +
                     " declare @p_id_etapa int" +
                     " declare @p_existe int" +
                     " declare @p_ValorUsuarioAsignado varchar(50)" +
                     " set  @p_ValorUsuarioAsignado =(select ValorUsuarioAsignado From Etapas" +
                     " where id = " + nuevaEtapa + "" +
                     " and ProcesoId = 1) " +

                    "  if (@p_ValorUsuarioAsignado= 'Mesa Control')" +
                    " begin" +
                     " set @p_unidadNegocio=null" +
                     " end" +
                     " else" +
                     " Begin" +
                     " set @p_unidadNegocio=(select top 1 UnidadNegocioAsignada " +
                    " from [" + _conf.GetValue<string>("schema") + @"].Tareas a inner" +
                    " join Solicitudes b on a.SolicitudId = b.Id" +
                    " where b.NumeroTicket = '" + creditos.NumeroTicket + "'" +
                    " and UnidadNegocioAsignada is not null" + " order by 1 asc) " +
                    "End" +
                    " set @p_id_solicitud = (select id from Solicitudes where NumeroTicket = '" + creditos.NumeroTicket + "')" +
                    " update a" +
                    " set a.EjecutadoPor = 'wfboot', a.Estado = 'Finalizada', a.FechaTerminoFinal = GETDATE()" +
                    " from [" + _conf.GetValue<string>("schema") + @"].Tareas a inner" +
                    " join [" + _conf.GetValue<string>("schema") + @"].Solicitudes b on a.SolicitudId = b.Id and a.estado='Activada'" +
                    " where b.NumeroTicket = '" + creditos.NumeroTicket + "'" +
                    " insert into [" + _conf.GetValue<string>("schema") + @"].Tareas(SolicitudId, EtapaId, AsignadoA, ReasignadoA, EjecutadoPor, Estado, FechaInicio, FechaTerminoEstimada, FechaTerminoFinal, UnidadNegocioAsignada)  " +
                    " values           (@p_id_solicitud,'" + nuevaEtapa + @"', @p_ValorUsuarioAsignado, null, null, 'Activada', GETDATE(), null, null, @p_unidadNegocio) ";


                con.Execute(actualizar.ToString(), null, null, 240);

            }
        }



        public int analizarDocumentosSc(string folioCredito)
         {
            var expedientes = _context.ExpedientesComplementarios.Where(a => a.FolioCredito == folioCredito);

            if (expedientes == null)//no existen en expedientes complementarios
                return 1;
            var cargariniciales = _context.CargasIniciales.Where(a => a.FolioCredito == folioCredito).FirstOrDefault();

            if (cargariniciales.CodigoOficinaIngreso != cargariniciales.CodigoOficinaPago)
            {
                // se agregan los documentos del credito
                var documentos = _context.Documentos.Include(d => d.ExpedienteCredito).ThenInclude(c => c.Credito).Where(c => c.ExpedienteCredito.Credito.FolioCredito == folioCredito && c.ExpedienteCredito.TipoExpediente == TipoExpediente.Complementario).FirstOrDefault();
                using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
                {
                    string sql = @"delete documentos where expedientecreditoid=" + documentos.ExpedienteCredito.Id;
                    con.Execute(sql.ToString(), null, null, 240);

                    var configuracion = _context.ConfiguracionDocumentos.Where(a => a.TipoExpediente == TipoExpediente.Complementario && a.Codificacion == "03" || a.Codificacion == "04" || a.Codificacion == "09").ToList();


                    foreach (var c in configuracion)
                    {
                        sql = @"
                         INSERT INTO [" + _conf.GetValue<string>("schema") + @"].[Documentos]
                         ([Resumen]
                         ,[Codificacion]
                         ,[TipoDocumento]
                         ,[ExpedienteCreditoId])
                         VALUES
                         ('" + c.TipoDocumento.ToString("D") + @"'
                         ,'" + c.Codificacion + @"'
                         ,'" + c.TipoDocumento + @"'
                         ,'" + documentos.ExpedienteCredito.Id + @"')";
                        con.Execute(sql.ToString(), null, null, 240);
                    }

                    
                    if (cargariniciales.Aval == "1")
                    {

                        var aval = _context.ConfiguracionDocumentos.Where(a => a.Codificacion == "05" && TipoExpediente.Complementario == a.TipoExpediente).FirstOrDefault();
                        sql = @"
                         INSERT INTO [" + _conf.GetValue<string>("schema") + @"].[Documentos]
                         ([Resumen]
                         ,[Codificacion]
                         ,[TipoDocumento]
                         ,[ExpedienteCreditoId])
                         VALUES
                         ('" + aval.TipoDocumento.ToString("D") + @"'
                         ,'" + aval.Codificacion + @"'
                         ,'" + aval.TipoDocumento + @"'
                         ,'" + documentos.ExpedienteCredito.Id + @"')";
                        con.Execute(sql.ToString(), null, null, 240);
                    }

                    if (cargariniciales.SeguroCesantia == "1")
                    {

                        var segurocesantia = _context.ConfiguracionDocumentos.Where(a => a.Codificacion == "10" && TipoExpediente.Complementario == a.TipoExpediente).FirstOrDefault();
                        sql = @"
                         INSERT INTO [" + _conf.GetValue<string>("schema") + @"].[Documentos]
                         ([Resumen]
                         ,[Codificacion]
                         ,[TipoDocumento]
                         ,[ExpedienteCreditoId])
                         VALUES
                         ('" + segurocesantia.TipoDocumento.ToString("D") + @"'
                         ,'" + segurocesantia.Codificacion + @"'
                         ,'" + segurocesantia.TipoDocumento + @"'
                         ,'" + documentos.ExpedienteCredito.Id + @"')";
                        con.Execute(sql.ToString(), null, null, 240);
                    }
                    if (cargariniciales.Afecto == "1")
                    {

                        var afecto = _context.ConfiguracionDocumentos.Where(a => a.Codificacion == "00" && TipoExpediente.Complementario == a.TipoExpediente).FirstOrDefault();
                        sql = @"
                         INSERT INTO [" + _conf.GetValue<string>("schema") + @"].[Documentos]
                         ([Resumen]
                         ,[Codificacion]
                         ,[TipoDocumento]
                         ,[ExpedienteCreditoId])
                         VALUES
                         ('" + afecto.TipoDocumento.ToString("D") + @"'
                         ,'" + afecto.Codificacion + @"'
                         ,'" + afecto.TipoDocumento + @"'
                         ,'" + documentos.ExpedienteCredito.Id + @"')";
                        con.Execute(sql.ToString(), null, null, 240);
                    }


                }

            }
            else
            {
                //se agregan solo los seguros
                var documentos = _context.Documentos.Include(d => d.ExpedienteCredito).ThenInclude(c => c.Credito).Where(c => c.ExpedienteCredito.Credito.FolioCredito == folioCredito && c.ExpedienteCredito.TipoExpediente == TipoExpediente.Complementario).FirstOrDefault();
                using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
                {
                    string sql = @"delete documentos where expedientecreditoid=" + documentos.ExpedienteCredito.Id;
                    con.Execute(sql.ToString(), null, null, 240);

                    var configuracion = _context.ConfiguracionDocumentos.Where(a => a.TipoExpediente == TipoExpediente.Complementario && a.Codificacion == "09").ToList();

                    foreach (var c in configuracion)
                    {
                        sql = @"
                         INSERT INTO [" + _conf.GetValue<string>("schema") + @"].[Documentos]
                         ([Resumen]
                         ,[Codificacion]
                         ,[TipoDocumento]
                         ,[ExpedienteCreditoId])
                         VALUES
                         ('" + c.TipoDocumento.ToString("D") + @"'
                         ,'" + c.Codificacion + @"'
                         ,'" + c.TipoDocumento + @"'
                         ,'" + documentos.ExpedienteCredito.Id + @"')";
                        con.Execute(sql.ToString(), null, null, 240);
                    }

                    if (cargariniciales.SeguroCesantia == "1")
                    {

                        var segurocesantia = _context.ConfiguracionDocumentos.Where(a => a.Codificacion == "10" && TipoExpediente.Complementario == a.TipoExpediente).FirstOrDefault();
                        sql = @"
                         INSERT INTO [" + _conf.GetValue<string>("schema") + @"].[Documentos]
                         ([Resumen]
                         ,[Codificacion]
                         ,[TipoDocumento]
                         ,[ExpedienteCreditoId])
                         VALUES
                         ('" + segurocesantia.TipoDocumento.ToString("D") + @"'
                         ,'" + segurocesantia.Codificacion + @"'
                         ,'" + segurocesantia.TipoDocumento + @"'
                         ,'" + documentos.ExpedienteCredito.Id + @"')";
                        con.Execute(sql.ToString(), null, null, 240);
                    }


                }
            }


            return 1;
        }



        public IEnumerable<dynamic> listarValijasEnviadasSc(string marcaAvance)
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
                    and exp.tipoExpediente=2 
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
        public IEnumerable<Documento> Documentos { get; set; }
        public DateTime FechaDesembolso { get; set; }
        public string seguimientoNotaria { get; set; }
        public DateTime fechaEnvioNotaria { get; set; }
        public string seguimientoValija { get; set; }
        public DateTime fechaEnvioValija { get; set; }
        public string reparo { get; set; }
        public string reparoNotaria { get; set; }
        public string documentosFaltantes { get; set; }
        public string codigoCajaValorada { get; set; }
        public string descipcionOficinapagadora { get; set; }
        public string descipcionOficina { get; set; }
    }




}