using System.Collections.Generic;
using System.Linq;

namespace GymMangment.BLL.ViewModels.WorkoutViewModels
{
    public class WorkoutExerciseViewModel
    {
        public int Id { get; set; }
        public string ExerciseName { get; set; } = default!;
        public ICollection<WorkoutSetViewModel> Sets { get; set; } = [];

        public decimal Volume => Sets.Sum(s => s.Weight * s.Reps);
        public decimal MaxWeight => Sets.Any() ? Sets.Max(s => s.Weight) : 0;
    }
}
