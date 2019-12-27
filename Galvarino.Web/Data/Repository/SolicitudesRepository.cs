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
                    
                    string sql = "SELECT  distinct      cr.FolioCredito, "+
                       " cr.RutCliente, "+
                       " cr.TipoCredito, "+
                       " cr.FechaDesembolso, "+
                       "  pkn.CodigoSeguimiento seguimientoNotaria, " +
                       " pkn.FechaEnvio fechaEnvioNotaria, " +
                       " vv.CodigoSeguimiento seguimientoValija,"+
                       " vv.FechaEnvio fechaEnvioValija,"+
                       " et.NombreInterno, "+
                       " ta.UnidadNegocioAsignada,"+ 
                       " ci.CodigoOficinaIngreso, "+
                       " ci.CodigoOficinaPago, "+
                       " o.Nombre descipcionOficina,"+
                       " opag.Nombre descipcionOficinapagadora,"+
                       " variab.Valor reparo"+
                       " FROM[dbo].[ExpedientesComplementarios]  ec" +
                       " INNER JOIN[dbo].[Creditos] cr ON cr.id = ec.creditoid" +
                       "  INNER JOIN[dbo].[Solicitudes] sl ON sl.NumeroTicket=ec.NumeroTicket" +
                       " INNER join[dbo].[Tareas] ta on ta.solicitudid=sl.Id" +
                       " INNER JOIN[dbo].[Etapas] et ON et.Id=ta.EtapaId" +
                       " INNER JOIN[dbo].[ExpedientesCreditos] ex ON ex.CreditoId = ec.creditoid" +
                       " LEFT JOIN[dbo].[PacksNotarias] pkn ON ex.PackNotariaId = pkn.Id"+
                       " LEFT JOIN[dbo].[ValijasValoradas] vv ON ex.ValijaValoradaId = vv.Id" +
                       "  INNER JOIN[dbo].CargasIniciales ci ON ci.FolioCredito = cr.FolioCredito"+
                       " INNER JOIN[dbo].Oficinas o ON ci.CodigoOficinaIngreso = o.Codificacion" +
                        "   INNER JOIN [dbo].Oficinas opag ON ci.CodigoOficinaPago = opag.Codificacion" +
                        " LEFT OUTER JOIN Variables variab on variab.NumeroTicket = ec.NumeroTicket and variab.Clave = 'DEVOLUCION_A_SUCURSAL_PAGO_SET_COMPLEMENTARIOS'"+
                       " where ta.Estado = 'Activada'" +
                       " and TipoExpediente=2" +
                      " and et.ProcesoId = 2"+
                       " and (ta.AsignadoA in (" + theRoles + ")  or ta.AsignadoA =  '" + rut + "')";
                   
                    if (theSteps == "'DESPACHO_A_DOCUMENTOS_A_SUCURSAL'")
                    {
                        sql = sql + " and  ci.CodigoOficinaIngreso!= ci.CodigoOficinaPago";

                    }
                    else if (theSteps == "'PREPARAR_NÓMINA_SET_COMPLEMENTARIO'")
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
                        sql = sql + " AND ( ci.CodigoOficinaIngreso= '" + sucursal + "' Or '" + sucursal + "'='AA00')";


                    }
                    sql = sql + " order by cr.folioCredito asc";


                     var item = con.Query<SolicitudResult>(sql, commandTimeout: 300, buffered: true);
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
                   select d.Id,d.Resumen,d.Codificacion,d.TipoDocumento,ExpedienteCreditoId
                   from ExpedientesCreditos ec
                   , Documentos d
                   ,ConfiguracionDocumentos cd
                   ,Creditos c
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
                    else if (theSteps == "'DESPACHO_A_DOCUMENTOS_A_SUCURSAL'")
                    {

                        respuesta.ForEach(s =>
                        {

                            string sql = @"
                   select d.Id,d.Resumen,d.Codificacion,d.TipoDocumento,ExpedienteCreditoId
                   from ExpedientesCreditos ec
                   , Documentos d
                   ,ConfiguracionDocumentos cd
                   ,Creditos c
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
                   from ExpedientesCreditos ec
                   , Documentos d
                   ,ConfiguracionDocumentos cd
                   ,Creditos c
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
                       " inner join Tareas t on t.EtapaId = e.id" +
                       " inner join Solicitudes s on s.id = t.SolicitudId" +
                       " inner join ExpedientesComplementarios ec on ec.NumeroTicket = s.NumeroTicket" +
                       " inner join CargasIniciales ci on ci.FolioCredito = ec.FolioCredito" +
                       " inner join Oficinas o on o.Codificacion = ci.CodigoOficinaIngreso" +
                       " where t.Estado = 'Activada'" +
                       " and ci.CodigoOficinaIngreso != ci.CodigoOficinaPago" +
                       " and t.EtapaId = 42";
                respuesta = con.Query<dynamic>(sql).AsList();
            }
            return respuesta;
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
                string sql = @"select Id,Nombre from oficinas order by 2";
                respuestaOficinas = con.Query<dynamic>(sql).AsList();

            }

            return respuestaOficinas;
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
            var RolVisualizador="";
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
             where vav.MarcaAvance = '" + marcaAvance + @"'
                   and exp.tipoExpediente=2";

                
                if (theSteps == "'RECEPCION_VALIJA_DOCUMENTOS_OFICINA_DESTINO'"  && RolVisualizador != "Mantenedor de Sistema")// valida perfil de visualcio de oficinas de ingreso
                {
                    sql = sql + " and ci.CodigoOficinaIngreso in (" + theOffices + @")";
                }
                if (theSteps == "'APERTURA_VALIJA_DOCUMENTOS_OFICINA_DESTINO'" && RolVisualizador != "Mantenedor de Sistema")// valida perfil de visualcio de oficinas de ingreso
                {
                    sql = sql + " and ci.CodigoOficinaIngreso in (" + theOffices + @")";
                }
              
                sql = sql + @" GROUP BY vav.CodigoSeguimiento,
                                    vav.FechaEnvio,
                                    ofc.Nombre";


                _logger.LogDebug(sql);
                respuesta = con.Query<dynamic>(sql).AsList();
            }
            return respuesta;
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