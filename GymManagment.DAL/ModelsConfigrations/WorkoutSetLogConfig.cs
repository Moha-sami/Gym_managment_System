using GymManagment.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagment.DAL.ModelsConfigrations
{
    internal class WorkoutSetLogConfig : IEntityTypeConfiguration<WorkoutSetLog>
    {
        public void Configure(EntityTypeBuilder<WorkoutSetLog> builder)
        {
            builder.HasKey(ws => ws.Id);
            builder.Property(ws => ws.SetNumber).IsRequired();
            builder.Property(ws => ws.Weight).IsRequired().HasPrecision(18, 2);
            builder.Property(ws => ws.Reps).IsRequired();

            // Exercise relationship
            builder.HasOne(ws => ws.WorkoutExerciseLog)
                   .WithMany(we => we.Sets)
                   .HasForeignKey(ws => ws.WorkoutExerciseLogId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
