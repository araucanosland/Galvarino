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
    public class CreditoConfig : IEntityTypeConfiguration<Credito>
    {
        public void Configure(EntityTypeBuilder<Credito> builder)
        {
            builder.HasIndex(field => field.NumeroTicket);
        }
    }
}
