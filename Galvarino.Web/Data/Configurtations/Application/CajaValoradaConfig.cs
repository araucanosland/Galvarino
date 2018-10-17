using Galvarino.Web.Models.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Galvarino.Web.Data.Configurtations.Application
{
    public class CajaValoradaConfig : IEntityTypeConfiguration<CajaValorada>
    {
        public void Configure(EntityTypeBuilder<CajaValorada> builder)
        {
            builder.HasMany(p => p.Expedientes)
                .WithOne(e => e.CajaValorada)
                .IsRequired(false);
        }
    }
}
