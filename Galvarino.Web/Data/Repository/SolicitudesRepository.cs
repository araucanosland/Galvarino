using Dapper;
using Galvarino.Web.Models.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
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


     


        public string MoverEtapa(string FolioCredito, string EtapaMover, string Encargado,string usuario)
        {
            using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
            {
                string actualizar = " declare @p_id_solicitud int " +
                    " declare @p_unidadNegocio varchar(5) " +
                    " declare @p_id_etapa int " +
                    " declare @p_existe int " +
                    " declare @p_numeroTicket varchar(40) " +
                    " declare @p_etapaid int " +
                    " declare @p_encargado varchar(20) " +
                    " select @p_etapaid = Id " +
                    " from Etapas " +
                    " where NombreInterno = '" + EtapaMover + "' " +
                    " set @p_numeroTicket = (select numeroticket from dbo.creditos where foliocredito = '" + FolioCredito + "') " +
                    "set @p_unidadNegocio = (select top 1 UnidadNegocioAsignada " +
                    " from Tareas a inner " +
                    " join Solicitudes b on a.SolicitudId = b.Id " +
                    " where b.numeroticket = @p_numeroTicket " +
                    " and UnidadNegocioAsignada is not null  order by 1 asc) " +
                    " set @p_id_solicitud = (select id from Solicitudes where NumeroTicket = @p_numeroTicket) " +
                    " set @p_existe = (select COUNT(*) from Tareas where SolicitudId = @p_id_solicitud and EtapaId = 18)  " +
                    " update a " +
                    " set a.EjecutadoPor = '"+usuario+"', a.Estado = 'Finalizada',a.FechaTerminoEstimada=GETDATE(), a.FechaTerminoFinal = GETDATE() " +
                    " from Tareas a inner " +
                    " join Solicitudes b on a.SolicitudId = b.Id and a.estado = 'Activada' " +
                     " and b.NumeroTicket = @p_numeroTicket " +
                    " insert into Tareas(SolicitudId, EtapaId, AsignadoA, ReasignadoA, EjecutadoPor, Estado, FechaInicio, FechaTerminoEstimada, FechaTerminoFinal, UnidadNegocioAsignada) " +
                    " values(@p_id_solicitud, @p_etapaid, '" + Encargado + "', null, null, 'Activada', GETDATE(), null, null, @p_unidadNegocio)";
                con.Execute(actualizar.ToString(), null, null, 240);
                return "OK";
            }

        }


        public IEnumerable<SolicitudAnalisisMC> ListarAnalisisMC(string[] roles, string nombreInterno)
        {

            var theRoles = "'" + String.Join("','", roles) + "'";

            var respuesta = new List<SolicitudAnalisisMC>();
            using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
            {
                string sql = @"select  c.FolioCredito,
                        c.RutCliente,
                        c.TipoCredito,                              
                        c.FechaDesembolso,
                        (select top 1 EjecutadoPor from
                        Tareas ta,
                        Solicitudes sol,
                        Creditos cre
                        where cre.NumeroTicket=sol.NumeroTicket
                        and sol.Id=ta.SolicitudId
                        and cre.FolioCredito=c.FolioCredito
                         and ta.Estado='Finalizada'
                        order by 1 ) EjecutadoPor
                        from Creditos c
                        ,Solicitudes s
                        ,Tareas t
                        ,Etapas e
                        where c.numeroticket=s.NumeroTicket
                        and s.Id=t.SolicitudId
                        and t.EtapaId=e.Id
                        and e.NombreInterno='ANALISIS_MESA_CONTROL'
                        and t.Estado='Activada'
                        ";
                respuesta = con.Query<SolicitudAnalisisMC>(sql).AsList();
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



        public IEnumerable<SolicitudAnalisisMCReparos> ListarReparos(string FolioCredito,string nombreInterno)
        {        

            var respuesta = new List<SolicitudAnalisisMCReparos>();
            using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
            {
                string sql = @"select  c.FolioCredito,
                            (select top 1 EjecutadoPor
                            from
                            Tareas ta,
                            Solicitudes sol,
                            Creditos cre
                            where cre.NumeroTicket=sol.NumeroTicket
                            and sol.Id=ta.SolicitudId
                            and cre.FolioCredito=c.FolioCredito
                            and ta.Estado='Finalizada'
                            order by 1 ) EjecutadoPor,
                            o.Nombre Oficina,
                            o.Codificacion codOficina
                            from Creditos c
                            ,Solicitudes s
                            ,Tareas t
                            ,Etapas e
                            ,CargasIniciales ci
                            ,Oficinas o
                            where c.numeroticket=s.NumeroTicket
                            and s.Id=t.SolicitudId
                            and t.EtapaId=e.Id
                            and e.NombreInterno='"+ nombreInterno + @"'
                            and t.Estado='Activada'
                            and ci.FolioCredito=c.FolioCredito
                            and ci.CodigoOficinaPago=o.Codificacion
                            and c.foliocredito='" + FolioCredito + "'";
                respuesta = con.Query<SolicitudAnalisisMCReparos>(sql).AsList();
            }
          

            return respuesta;

        }


        public IEnumerable<dynamic> listarResumenInicial2(string[] roles, string rut, string[] oficinas)
        {
            var theRoles = "'" + String.Join("','", roles) + "'";
            var theOffices = "'" + String.Join("','", oficinas) + "'";

            var respuesta = new List<dynamic>();
            using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
            {

                 string sql = @"
                   declare @p_roleAgente int
                    set @p_roleAgente = (select COUNT(*) from Usuarios u,
                    rolesusuarios ru,Roles r
                    where u.Id = ru.UserId
                    and u.Identificador = '" + rut + @"'
                    and r.Id = ru.RoleId
                    and r.NormalizedName = 'AGENTE')

                     if (@p_roleAgente = 1)
                     Begin
                     select COUNT(*) cantidad ,o.Nombre oficina from
                     [" + _conf.GetValue<string>("schema") + @"].Creditos c
                     ,[" + _conf.GetValue<string>("schema") + @"].Solicitudes s
                     ,[" + _conf.GetValue<string>("schema") + @"].Tareas t
                     ,[" + _conf.GetValue<string>("schema") + @"].Etapas e
                     ,[" + _conf.GetValue<string>("schema") + @"].CargasIniciales ci
                     ,[" + _conf.GetValue<string>("schema") + @"].Oficinas o
                     where c.NumeroTicket = s.NumeroTicket
                     and s.Id = t.SolicitudId
                     and e.Id = t.EtapaId
                     and t.Estado = 'Activada'
                     and e.NombreInterno = 'SOLUCION_REPAROS_SUCURSAL'
                     and ci.foliocredito = c.foliocredito
                     and o.Codificacion = ci.CodigoOficinaPago
                     group by o.Nombre
                      End
                        ELSE
                       Begin
                    select COUNT(*) cantidad, o.Nombre oficina from
                    [" + _conf.GetValue<string>("schema") + @"].Creditos c
                    ,[" + _conf.GetValue<string>("schema") + @"].Solicitudes s
                    ,[" + _conf.GetValue<string>("schema") + @"].Tareas t
                    ,[" + _conf.GetValue<string>("schema") + @"].Etapas e
                    ,[" + _conf.GetValue<string>("schema") + @"].CargasIniciales ci
                    ,[" + _conf.GetValue<string>("schema") + @"].Oficinas o
                    where c.NumeroTicket = s.NumeroTicket
                    and s.Id = t.SolicitudId
                    and e.Id = t.EtapaId
                    and t.Estado = 'Activada'
                    and ci.foliocredito = c.foliocredito
                    and o.Codificacion = ci.CodigoOficinaPago
                    and ((t.AsignadoA in (" + theRoles + @") or t.AsignadoA = '" + rut + @"')
                    and ((t.UnidadNegocioAsignada in (" + theOffices + @") or t.UnidadNegocioAsignada is null))
                    and e.NombreInterno = 'SOLUCION_REPAROS_SUCURSAL')
                    group by o.Nombre
                      End";

                respuesta = con.Query<dynamic>(sql).AsList();
            }



            return respuesta;
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
                 from[dbo].Tareas ta
                 inner join[dbo].[Etapas] et on ta.EtapaId = et.Id
                 inner join[dbo].[Solicitudes] sl on ta.SolicitudId = sl.Id
                 inner join[dbo].[Creditos] cr on sl.NumeroTicket = cr.NumeroTicket
                 inner join[dbo].[ExpedientesCreditos] ex on cr.Id = ex.CreditoId
                 where ta.Estado = 'Activada'
                 and ((ta.AsignadoA in (" + theRoles + @") or ta.AsignadoA = '" + rut + @"')
                 and ((ta.UnidadNegocioAsignada in (" + theOffices + @")) or ta.UnidadNegocioAsignada = null)
                    )
                GROUP BY et.Nombre";
                 

                respuesta = con.Query<dynamic>(sql).AsList();
            }



            return respuesta;
        }

        public IEnumerable<dynamic> listarOficinas(string EsRM)
        {
            var respuestaOficinas = new List<dynamic>();
            using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
            {
                string sql = @"select Id,Nombre from oficinas order by 2 ";
                respuestaOficinas = con.Query<dynamic>(sql).AsList();

            }

            return respuestaOficinas;
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
                            vdf.Valor documentosFaltantes,
                            vds.Valor descripcionReparos

                    from [" + _conf.GetValue<string>("schema") + @"].[Tareas] ta  with (nolock) 
                    inner join [" + _conf.GetValue<string>("schema") + @"].[Etapas] et  with (nolock)  on ta.EtapaId = et.Id
                    inner join [" + _conf.GetValue<string>("schema") + @"].[Solicitudes] sl  with (nolock) on ta.SolicitudId = sl.Id
                    inner join [" + _conf.GetValue<string>("schema") + @"].[Creditos] cr  with (nolock) on sl.NumeroTicket = cr.NumeroTicket
                    inner join [" + _conf.GetValue<string>("schema") + @"].[ExpedientesCreditos] ex  with (nolock) on cr.Id = ex.CreditoId
                    left join [" + _conf.GetValue<string>("schema") + @"].[PacksNotarias] pkn  with (nolock) on ex.PackNotariaId = pkn.Id
                    left join [" + _conf.GetValue<string>("schema") + @"].[ValijasValoradas] vv  with (nolock) on ex.ValijaValoradaId = vv.Id
                    left join [" + _conf.GetValue<string>("schema") + @"].[Variables] vre  with (nolock) on sl.NumeroTicket = vre.NumeroTicket and vre.Clave = 'CODIGO_MOTIVO_DEVOLUCION_A_SUCURSAL'
                    left join [" + _conf.GetValue<string>("schema") + @"].[Variables] vrn  with (nolock) on sl.NumeroTicket = vrn.NumeroTicket and vrn.Clave = 'REPARO_REVISION_DOCUMENTO_LEGALIZADO'
                    left join [" + _conf.GetValue<string>("schema") + @"].[Variables] vdf  with (nolock) on sl.NumeroTicket = vdf.NumeroTicket and vdf.Clave = 'COLECCION_DOCUMENTOS_FALTANTES'
                    left join [" + _conf.GetValue<string>("schema") + @"].[Variables] vds  with (nolock) on sl.NumeroTicket = vds.NumeroTicket and vds.Clave = 'DESCRIPCION_REPARO'
                    where ta.Estado = 'Activada'
                    and ((ta.AsignadoA in (" + theRoles + @") or ta.AsignadoA = '" + rut + @"')
                            and ((ta.UnidadNegocioAsignada in (" + theOffices + @" ) or ta.UnidadNegocioAsignada is null))
                            and (et.NombreInterno in (" + theSteps + @"))
                        )
                    and ('" + notaria + @"' = '' or pkn.NotariaEnvioId = '" + notaria + @"')    
                    " + sqlTrozoFechaConsulta + @"
                    order by " + (order == null ? "cr.FechaDesembolso" : order);
                _logger.LogDebug(sql);
                
                respuesta = con.Query<SolicitudResult>(sql,null,null,true,360).AsList();
            }


            respuesta.ForEach(s =>
            {
                s.Documentos = _context.Documentos
                               .Include(d => d.ExpedienteCredito)
                               .ThenInclude(f => f.Credito)
                               .Where(d => d.ExpedienteCredito.Credito.FolioCredito == s.FolioCredito);
            });

            return respuesta;
        }



       
        public IEnumerable<SolicitudlistarValijasEnviadas> listarValijasEnviadas(string marcaAvance)
        {
            var respuesta = new List<SolicitudlistarValijasEnviadas>();
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
                respuesta = con.Query<SolicitudlistarValijasEnviadas>(sql).AsList();
            }
            return respuesta;
        }

        public IEnumerable<dynamic> ActualizarEtapa(string rut, int solicitudid, int idetapa, string folio, bool tienevalija)
        {
            var respuesta = new List<dynamic>();
            using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
            {
                string actualizar = "declare @idvalijaold integer, @creditoid integer, @cantvalijas integer" +
                     " update Tareas" +
                     " set EjecutadoPor = '" + rut + "', Estado = 'Finalizada', FechaTerminoFinal = GETDATE()" +
                     " where SolicitudId = '" + solicitudid + "'" + " and estado = 'Activada'" +
                     " insert into Tareas(SolicitudId, EtapaId, AsignadoA, ReasignadoA, EjecutadoPor, Estado, FechaInicio, FechaTerminoEstimada, FechaTerminoFinal, UnidadNegocioAsignada)  " +
                     " values           ('" + solicitudid + "', '" + idetapa + "', 'Mesa Control', null,  null , 'Activada', GETDATE(), null, null, null) ";
                if (tienevalija)
                {
                    actualizar = actualizar +
                    " set @creditoid = (select Id from Creditos where FolioCredito =  '" + folio + "') " +
                    " set @idvalijaold = (select ValijaValoradaId from ExpedientesCreditos where CreditoId = @creditoid) " +
                    " insert into Credito_ValijaValorada values(@creditoid, @idvalijaold) " +
                    " update ExpedientesCreditos set ValijaValoradaId = NULL where CreditoId = @creditoid " +
                    " set @cantvalijas = (select count(*) from ExpedientesCreditos where ValijaValoradaId = @idvalijaold) " +
                    " if @cantvalijas = 0 " +
                    "      delete ValijasValoradas  where Id = @idvalijaold ";
                }
                actualizar = actualizar +
                " select 'Actualización ejecutada' as salida ";

                _logger.LogDebug(actualizar);
                respuesta = con.Query<dynamic>(actualizar).AsList();
            }
            return respuesta;
        }

        public IEnumerable<ReporteProgramado> ListaRegistroReporteProgramado ()
        {
            var respuesta = new List<ReporteProgramado>();
            using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
            {
                string sql = @"SELECT * FROM [dbo].[ReporteProgramado] ";
                respuesta = con.Query<ReporteProgramado>(sql).AsList();

               //if (respuesta)

            }
            return respuesta;
        }

        public IEnumerable<dynamic> ReporteGestion(DateTime fechaInicial, DateTime fechaFinal)
        {
            try
            {
                var respuesta = new List<dynamic>();
                using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
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
                                    and ci.FechaCorresponde >= '2022-08-18 00:00:00.0000000' 
                                    and ci.FechaCorresponde <= '2022-09-19 00:00:00.0000000'
                                    )reporte
                                    where reporte.rnk=1
                                    ";
                    respuesta = con.Query<dynamic>(sql).AsList();
                }
                return respuesta;
            }
            catch (Exception ex)
            {

                throw ;
            }
           
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
        public string descripcionReparos { get; set; }
    }

    public class SolicitudAnalisisMC
    {
        public string FolioCredito { get; set; }
        public string RutCliente { get; set; }
        public int TipoCredito { get; set; }
        public IEnumerable<Documento> Documentos { get; set; }
        public DateTime FechaDesembolso { get; set; }
        public string EjecutadoPor { get; set; }
    }

    public class SolicitudAnalisisMCReparos
    {
        public string FolioCredito { get; set; }
        public string Oficina { get; set; }
        public string EjecutadoPor { get; set; }
        public string CodOficina { get; set; }
    }

    public class SolicitudlistarValijasEnviadas
    {
        public string CodigoSeguimiento { get; set; }
        public DateTime FechaEnvio { get; set; }
        public string NombreOficina { get; set; }
        public string NroExpedientes { get; set; }
        
      
    }

}