using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Galvarino.Web.Models.Application;
using Galvarino.Web.Models.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Galvarino.Web.Data.Configurtations.Workflow
{
    public class ExpedientesComplementariosConfig : IEntityTypeConfiguration<ExpedienteComplementario>
    {
        public void Configure(EntityTypeBuilder<ExpedienteComplementario> builder)
        {
            builder.HasIndex(field => field.NumeroTicket);
        }

    }
}
