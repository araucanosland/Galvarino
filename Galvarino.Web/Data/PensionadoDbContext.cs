using Galvarino.Web.Data.Configurtations.Application;
using Galvarino.Web.Models.Application.Pensionado;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Galvarino.Web.Data
{

    public class PensionadoDbContext : DbContext
    {


        public DbSet<HomologacionOficinas> HomologacionOficinas { get; set; }

        public DbSet<CargasIniciales> CargasIniciales { get; set; }


        public DbSet<CargasInicialesEstado> CargasInicialesEstado { get; set; }

        public DbSet<Pensionado> Pensionado { get; set; }

        public DbSet<Expedientes> Expedientes { get; set; }

        public DbSet<Tipo> Tipo { get; set; }

        public DbSet<Sucursal> Sucursal { get; set; }

        public DbSet<Procesos> Procesos { get; set; }

        public DbSet<Solicitudes> Solicitudes { get; set; }

        public DbSet<ConfiguracionDocumentos> ConfiguracionDocumentos { get; set; }

        public DbSet<Documentos> Documentos { get; set; }

        public DbSet<LogCargaInicial> LogCargaInicial { get; set; }

        public DbSet <Tareas> Tareas{ get; set; }

        public DbSet <Etapas> Etapas { get; set; }

        private readonly IConfiguration _conf;

        public PensionadoDbContext(DbContextOptions<PensionadoDbContext> options, IConfiguration conf)
            : base(options)
        {
            _conf = conf;
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema(_conf.GetValue<string>("schemaPensionado"));
           
           /*Configurar Modelos */
            builder.ApplyConfiguration(new HomologacionOficinaConfig());
        }
    }
}
