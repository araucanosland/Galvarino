using System;
using System.Collections.Generic;
using Galvarino.Web.Models.Application;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace Galvarino.Web.Data.Repository
{
    public class SolicitudesRepository
    {
        private readonly IConfiguration _conf;
        public SolicitudesRepository(IConfiguration conf)
        {
            _conf = conf;
        }

        public IEnumerable<SolicitudResult> listarSolicitudes()
         {
             var respuesta = new List<SolicitudResult>();
             using (var con = new SqlConnection(_conf.GetConnectionString("DocumentManagementConnection")))
             {
                 string sql = @"
                    SELECT *
                    FROM 
                 ";
                 con.Query<SolicitudResult>(sql);
             }

             return respuesta;
         }
    }


    public class SolicitudResult
    {
        public string FolioCredito { get; set; }
        public string RutCliente { get; set; }
        public string TipoCredito { get; set; }
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