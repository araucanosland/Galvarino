using Galvarino.Web.Data.Configurtations.Application;
using Galvarino.Web.Models.Application.Pensionado;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Galvarino.Web.Data
{

    public class PensionadoDbContext : DbContext
    {


        public DbSet<HomologacionOficinas> HomologacionOficinas { get; set; }

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
