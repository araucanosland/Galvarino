using Galvarino.Web.Models.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Data.Configurtations.Security
{
    public class OrganizacionConfig : IEntityTypeConfiguration<Organizacion>
    {
        public void Configure(EntityTypeBuilder<Organizacion> builder)
        {
            builder.HasMany(f => f.Roles)
                .WithOne(f => f.Orzanizacion)
                .IsRequired();

            builder.HasOne(f => f.Padre)
                .WithMany(e => e.Hijos)
                .IsRequired(false);

            builder.Property(p => p.Nombre)
                .IsRequired();

            builder.Property(o => o.TipoOrganizacion)
                .HasConversion(
                    c => ((char)c).ToString(),
                    c => (TipoOrganizacion)char.Parse(c)
                )
                .IsRequired();
        }
    }
}
