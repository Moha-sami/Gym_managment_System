using GymManagment.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagment.DAL.ModelsConfigrations
{
    internal class WorkoutLogConfig : IEntityTypeConfiguration<WorkoutLog>
    {
        public void Configure(EntityTypeBuilder<WorkoutLog> builder)
        {
            builder.HasKey(w => w.Id);
            builder.Property(w => w.Name).IsRequired().HasMaxLength(100);
            builder.Property(w => w.Date).IsRequired();
            builder.Property(w => w.Notes).HasMaxLength(500);

            // Member relationship
            builder.HasOne(w => w.Member)
                   .WithMany(m => m.WorkoutLogs)
                   .HasForeignKey(w => w.MemberId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
