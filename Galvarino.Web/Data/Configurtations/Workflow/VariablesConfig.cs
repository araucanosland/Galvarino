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
    public class VariablesConfig : IEntityTypeConfiguration<Variable>
    {
        public void Configure(EntityTypeBuilder<Variable> builder)
        {
            builder.HasIndex(field => new { field.NumeroTicket, field.Clave });

        }
    }
}
