using Galvarino.Web.Models.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Galvarino.Web.Data.Configurtations.Application
{
    public class NotariaConfig : IEntityTypeConfiguration<Notaria>
    {
        public void Configure(EntityTypeBuilder<Notaria> builder)
        {
            builder.HasOne(n => n.Comuna).WithMany().IsRequired();
        }
    }
}
