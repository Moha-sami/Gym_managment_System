using GymMangment.BLL.ViewModels.WorkoutPlanViewModels;
using System.Threading.Tasks;

namespace GymMangment.BLL.Services.Interfaces
{
    public interface IWorkoutPlanService
    {
        Task<WorkoutPlanViewModel?> GetActivePlanAsync(string userEmail);
        Task<WorkoutPlanViewModel> GenerateAndSavePlanAsync(string userEmail, string goal, int frequency, string experienceLevel);
        Task<bool> LogPlanDayToJournalAsync(string userEmail, int dayNumber);
    }
}
