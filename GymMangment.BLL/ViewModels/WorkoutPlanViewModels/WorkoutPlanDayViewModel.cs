using System.Collections.Generic;

namespace GymMangment.BLL.ViewModels.WorkoutPlanViewModels
{
    public class WorkoutPlanDayViewModel
    {
        public int DayNumber { get; set; }
        public string DayName { get; set; } = string.Empty;
        public List<WorkoutPlanExerciseViewModel> Exercises { get; set; } = new();
    }
}
