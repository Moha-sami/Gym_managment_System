using GymManagment.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagment.DAL.ModelsConfigrations
{
    public class plansConfig : IEntityTypeConfiguration<Plans>
    {
        public void Configure(EntityTypeBuilder<Plans> builder)
        {
           builder.Property(p=>p.Name)
                .HasColumnType("varchar").HasMaxLength(50);

            builder.Property(p=>p.Description)
                .HasColumnType("varchar").HasMaxLength(200);

            builder.Property(p => p.Price)
                .HasPrecision(6, 2);

            builder.Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.ToTable(TB =>
            {
                TB.HasCheckConstraint("CK_Plan_DurationInDays", "DurationInDays between 1 and 365");
            });
        }
    }
}
