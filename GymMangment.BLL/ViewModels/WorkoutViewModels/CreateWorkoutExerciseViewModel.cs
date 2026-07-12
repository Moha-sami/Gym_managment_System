using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GymMangment.BLL.ViewModels.WorkoutViewModels
{
    public class CreateWorkoutExerciseViewModel
    {
        [Required(ErrorMessage = "Exercise name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Exercise name must be between 1 and 100 characters")]
        public string ExerciseName { get; set; } = default!;

        public List<CreateWorkoutSetViewModel> Sets { get; set; } = [];
    }
}
