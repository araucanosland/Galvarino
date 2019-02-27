using Galvarino.Workflow.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Workflow.Infrastructure.Configurtations
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
