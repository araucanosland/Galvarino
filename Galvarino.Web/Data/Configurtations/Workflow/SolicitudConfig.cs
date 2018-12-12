using Galvarino.Web.Models.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Galvarino.Web.Data.Configurtations.Workflow
{
    public class SolicitudConfig : IEntityTypeConfiguration<Solicitud>
    {
        public void Configure(EntityTypeBuilder<Solicitud> builder)
        {
            builder.Property(d => d.Estado)
               .HasConversion(new ValueConverter<EstadoSolicitud, string>(
                   v => v.ToString(),
                   v => (EstadoSolicitud)Enum.Parse(typeof(EstadoSolicitud), v))
               )
               .IsRequired();

            builder.HasMany(d => d.Tareas)
                .WithOne(d => d.Solicitud)
                .IsRequired();

            builder.Property(s => s.NumeroTicket)
                .IsRequired();

            builder.Property(s => s.FechaTermino)
                .IsRequired(false);

            builder.HasIndex(s => s.NumeroTicket)
               .IsUnique();

            builder.Property(s => s.InstanciadoPor)
                .IsRequired();

            builder.HasIndex(s => s.InstanciadoPor);

            builder.Property(s => s.Estado)
                .HasMaxLength(200);

        }
    }
}
