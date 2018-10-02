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
    public class DocumentoConfig : IEntityTypeConfiguration<Documento>
    {
        public void Configure(EntityTypeBuilder<Documento> builder)
        {
            builder.Property(d=>d.TipoDocumento)
                .HasConversion(new ValueConverter<TipoDocumento, string>(
                   v => v.ToString(),
                   v => (TipoDocumento)Enum.Parse(typeof(TipoDocumento), v))
               )
               .IsRequired();


            builder.Property(d => d.Codificacion).IsRequired();
        }
    }
}
