using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Web.Models.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Galvarino.Web.Data.Configurtations.Workflow
{
    public class TareaConfig : IEntityTypeConfiguration<Tarea>
    {
        public void Configure(EntityTypeBuilder<Tarea> builder)
        {
            builder.Property(d => d.Estado)
                .HasConversion(
                new ValueConverter<EstadoTarea, string>(
                    v => v.ToString(),
                    v => (EstadoTarea)Enum.Parse(typeof(EstadoTarea), v))
                )
                .IsRequired();
                

            builder.Property(d => d.Estado)
               .HasMaxLength(200);

            builder.HasIndex(field => new {field.AsignadoA, field.Estado, field.UnidadNegocioAsignada});
            
        }
    }
}
