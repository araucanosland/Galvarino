using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Web.Models.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Galvarino.Web.Data.Configurtations.Application
{
    public class OficinaConfig : IEntityTypeConfiguration<Oficina>
    {
        public void Configure(EntityTypeBuilder<Oficina> builder)
        {
            builder.HasOne(o => o.Comuna).WithMany();

            builder.HasOne(o => o.OficinaProceso)
                .WithMany()
                .IsRequired();

            builder.HasMany(o => o.PacksNotaria)
                .WithOne(p => p.Oficina)
                .IsRequired();

        }
    }
}
