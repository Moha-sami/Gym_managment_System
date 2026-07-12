using System.ComponentModel.DataAnnotations;

namespace GymMangment.BLL.ViewModels.WorkoutViewModels
{
    public class CreateWorkoutSetViewModel
    {
        [Required(ErrorMessage = "Set number is required")]
        [Range(1, 100, ErrorMessage = "Set number must be between 1 and 100")]
        public int SetNumber { get; set; }

        [Required(ErrorMessage = "Weight is required")]
        [Range(0.1, 1000, ErrorMessage = "Weight must be between 0.1 and 1000 kg")]
        public decimal Weight { get; set; }

        [Required(ErrorMessage = "Reps is required")]
        [Range(1, 1000, ErrorMessage = "Reps must be between 1 and 1000")]
        public int Reps { get; set; }
    }
}
