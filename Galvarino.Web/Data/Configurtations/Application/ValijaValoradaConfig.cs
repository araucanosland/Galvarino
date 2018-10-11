using Galvarino.Web.Models.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Galvarino.Web.Data.Configurtations.Application
{
    public class ValijaValoradaConfig : IEntityTypeConfiguration<ValijaValorada>
    {
        public void Configure(EntityTypeBuilder<ValijaValorada> builder)
        {
            builder.HasMany(p => p.Expedientes)
                .WithOne(e => e.ValijaValorada)
                .IsRequired(false);
        }
    }
}
