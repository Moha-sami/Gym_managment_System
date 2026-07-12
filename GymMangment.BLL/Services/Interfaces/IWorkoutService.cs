using GymMangment.BLL.Common;
using GymMangment.BLL.ViewModels.WorkoutViewModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GymMangment.BLL.Services.Interfaces
{
    public interface IWorkoutService
    {
        Task<Result<IEnumerable<WorkoutLogViewModel>>> GetMemberWorkoutsAsync(int memberId, CancellationToken ct = default);
        Task<Result<WorkoutLogViewModel?>> GetWorkoutDetailsAsync(int id, int memberId, CancellationToken ct = default);
        Task<Result> SaveWorkoutAsync(CreateWorkoutLogViewModel model, CancellationToken ct = default);
        Task<Result> DeleteWorkoutAsync(int id, int memberId, CancellationToken ct = default);
    }
}
