using System.Collections.Generic;

namespace GymManagment.DAL.Models
{
    public class WorkoutExerciseLog : BaseEntity
    {
        public string ExerciseName { get; set; } = default!;

        // Relations
        public int WorkoutLogId { get; set; }
        public WorkoutLog WorkoutLog { get; set; } = default!;

        public ICollection<WorkoutSetLog> Sets { get; set; } = [];
    }
}
