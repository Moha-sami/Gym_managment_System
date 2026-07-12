using GymManagment.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagment.DAL.ModelsConfigrations
{
    internal class WorkoutExerciseLogConfig : IEntityTypeConfiguration<WorkoutExerciseLog>
    {
        public void Configure(EntityTypeBuilder<WorkoutExerciseLog> builder)
        {
            builder.HasKey(we => we.Id);
            builder.Property(we => we.ExerciseName).IsRequired().HasMaxLength(100);

            // Workout relationship
            builder.HasOne(we => we.WorkoutLog)
                   .WithMany(w => w.Exercises)
                   .HasForeignKey(we => we.WorkoutLogId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
