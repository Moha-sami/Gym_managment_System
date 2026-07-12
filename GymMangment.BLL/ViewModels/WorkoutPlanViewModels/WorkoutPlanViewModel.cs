using System.Collections.Generic;

namespace GymMangment.BLL.ViewModels.WorkoutPlanViewModels
{
    public class WorkoutPlanViewModel
    {
        public int Id { get; set; }
        public string Goal { get; set; } = string.Empty;
        public int Frequency { get; set; }
        public string ExperienceLevel { get; set; } = string.Empty;
        public List<WorkoutPlanDayViewModel> Days { get; set; } = new();
    }
}
