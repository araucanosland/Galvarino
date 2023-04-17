using Galvarino.Web.Models.Application.Pensionado;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Galvarino.Web.Data.Configurtations.Application
{
    public class HomologacionOficinaConfig : IEntityTypeConfiguration<HomologacionOficinas>
    {
        public void Configure(EntityTypeBuilder<HomologacionOficinas> builder)
        {
         
                builder.HasOne(e => e.IdSucursalActividad).WithOne().IsRequired();
                builder.Property(f => f.IdOficina).IsRequired();
        }
    }
}
