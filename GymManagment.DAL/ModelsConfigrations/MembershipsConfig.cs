using GymManagment.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagment.DAL.ModelsConfigrations
{
    public class MembershipsConfig : IEntityTypeConfiguration<Membership>
    {
        public void Configure(EntityTypeBuilder<Membership> builder)
        {
           builder.HasKey(x => x.Id);
            builder.Property(x => x.CreatedAt).HasColumnName("StartDate")
                .HasDefaultValueSql("GETDATE()");
        }
    }
}
