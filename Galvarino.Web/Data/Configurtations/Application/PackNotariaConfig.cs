using Galvarino.Web.Models.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Galvarino.Web.Data.Configurtations.Application
{
    public class PackNotariaConfig : IEntityTypeConfiguration<PackNotaria>
    {
        public void Configure(EntityTypeBuilder<PackNotaria> builder)
        {
            builder.HasMany(p => p.Expedientes)
                .WithOne(e => e.PackNotaria)
                .IsRequired(false);

            builder.HasOne(p => p.NotariaEnvio).WithMany().IsRequired();
        }
    }
}
