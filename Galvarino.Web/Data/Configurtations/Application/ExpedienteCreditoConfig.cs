using Galvarino.Web.Models.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Galvarino.Web.Data.Configurtations.Application
{
    public class ExpedienteCreditoConfig : IEntityTypeConfiguration<ExpedienteCredito>
    {
        public void Configure(EntityTypeBuilder<ExpedienteCredito> builder)
        {
            builder.HasOne(e => e.Credito).WithMany().IsRequired();

            builder.HasMany(e => e.Documentos).WithOne(x => x.ExpedienteCredito).IsRequired();
        }
    }
}
