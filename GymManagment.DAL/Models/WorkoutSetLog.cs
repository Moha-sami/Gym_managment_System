namespace GymManagment.DAL.Models
{
    public class WorkoutSetLog : BaseEntity
    {
        public int SetNumber { get; set; }
        public decimal Weight { get; set; }
        public int Reps { get; set; }

        // Relations
        public int WorkoutExerciseLogId { get; set; }
        public WorkoutExerciseLog WorkoutExerciseLog { get; set; } = default!;
    }
}
