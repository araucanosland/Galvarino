using Galvarino.Web.Models.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Data.Configurtations.Application
{
    public class ConfiguracionDocumentoConfig : IEntityTypeConfiguration<ConfiguracionDocumento>
    {
        public void Configure(EntityTypeBuilder<ConfiguracionDocumento> builder)
        {
            builder.Property(d => d.TipoDocumento)
                .HasConversion(new ValueConverter<TipoDocumento, string>(
                   v => v.ToString(),
                   v => (TipoDocumento)Enum.Parse(typeof(TipoDocumento), v))
               )
               .IsRequired();


            builder.Property(d => d.TipoExpediente)
                .HasConversion(new ValueConverter<TipoExpediente, string>(
                   v => v.ToString(),
                   v => (TipoExpediente)Enum.Parse(typeof(TipoExpediente), v))
               )
               .IsRequired();

            builder.Property(d => d.TipoCredito)
             .HasConversion(new ValueConverter<TipoCredito, string>(
                v => v.ToString(),
                v => (TipoCredito)Enum.Parse(typeof(TipoCredito), v))
            )
            .IsRequired();
        }
    }
}
