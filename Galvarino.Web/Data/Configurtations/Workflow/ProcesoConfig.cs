using Galvarino.Web.Models.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Data.Configurtations.Workflow
{
    public class ProcesoConfig : IEntityTypeConfiguration<Proceso>
    {
        public void Configure(EntityTypeBuilder<Proceso> builder)
        {
            builder.HasMany(d => d.Etapas)
                .WithOne(d => d.Proceso)
                .IsRequired();

            builder.Property(p => p.Nombre)
                .IsRequired()
                .HasMaxLength(150);
        }
    }
}
