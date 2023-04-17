using Galvarino.Web.Models.Application.Pensionado;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Galvarino.Web.Data
{
    public class PensionadoDbContext : DbContext
    {

        #region Pensionado
        public DbSet<HomologacionOficinas> HomologacionOficinas { get; set; }

        #endregion

        private readonly IConfiguration _conf;

        public PensionadoDbContext(DbContextOptions<PensionadoDbContext> options, IConfiguration conf)
            : base(options)
        {
            _conf = conf;
        }

        public PensionadoDbContext(IConfiguration conf)
        {
            _conf = conf;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema(_conf.GetValue<string>("schemaPensionado"));
        }



    }
}
