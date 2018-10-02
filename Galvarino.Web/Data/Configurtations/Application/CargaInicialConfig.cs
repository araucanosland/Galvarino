using Galvarino.Web.Models.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Data.Configurtations.Application
{
    public class CargaInicialConfig : IEntityTypeConfiguration<CargaInicial>
    {
        public void Configure(EntityTypeBuilder<CargaInicial> builder)
        {
            builder.Property(c => c.FolioCredito)
                .IsRequired();

            builder.Property(c => c.FechaCarga)
                .IsRequired();

            builder.Property(c => c.FechaCorresponde)
                .IsRequired();
        }
    }
}
