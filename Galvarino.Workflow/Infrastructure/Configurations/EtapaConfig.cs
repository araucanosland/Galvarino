using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Workflow.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Galvarino.Workflow.Infrastructure.Configurtations
{
    public class EtapaConfig : IEntityTypeConfiguration<Etapa>
    {
        public void Configure(EntityTypeBuilder<Etapa> builder)
        {
            builder.HasMany(d => d.TareasAutomaticas)
               .WithOne(d => d.Etapa);

            builder.HasMany(d => d.Destinos)
                .WithOne(d => d.EtapaDestino)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(d => d.Actuales)
                .WithOne(d => d.EtapaActaual)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(e => e.TipoUsuarioAsignado)
                .HasConversion(new ValueConverter<TipoUsuarioAsignado, string>(
                    v => v.ToString(),
                    v => (TipoUsuarioAsignado)Enum.Parse(typeof(TipoUsuarioAsignado), v))
                ).IsRequired();


            builder.Property(d => d.TipoEtapa)
                .HasConversion(new ValueConverter<TipoEtapa, string>(
                    v => v.ToString(),
                    v => (TipoEtapa)Enum.Parse(typeof(TipoEtapa), v))
                ).IsRequired();

            builder.Property(d => d.TipoDuracion)
                .HasConversion(new ValueConverter<TipoDuracion, string>(
                    v => v.ToString(),
                    v => (TipoDuracion)Enum.Parse(typeof(TipoDuracion), v))
                );

            builder.Property(d => d.TipoDuracionRetardo)
                .HasConversion(new ValueConverter<TipoDuracion, string>(
                    v => v.ToString(),
                    v => (TipoDuracion)Enum.Parse(typeof(TipoDuracion), v))
                );

            builder.Property(s => s.ValorUsuarioAsignado)
                .IsRequired();

            builder.HasIndex(field => new {field.NombreInterno, field.TipoEtapa, field.TipoUsuarioAsignado});


        }
    }
}
