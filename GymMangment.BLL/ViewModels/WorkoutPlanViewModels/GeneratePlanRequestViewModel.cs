using System.ComponentModel.DataAnnotations;

namespace GymMangment.BLL.ViewModels.WorkoutPlanViewModels
{
    public class GeneratePlanRequestViewModel
    {
        [Required(ErrorMessage = "Goal is required.")]
        public string Goal { get; set; } = string.Empty;

        [Required(ErrorMessage = "Frequency is required.")]
        [Range(3, 5, ErrorMessage = "Frequency must be between 3 and 5 days per week.")]
        public int Frequency { get; set; }

        [Required(ErrorMessage = "Experience Level is required.")]
        public string ExperienceLevel { get; set; } = string.Empty;
    }
}
