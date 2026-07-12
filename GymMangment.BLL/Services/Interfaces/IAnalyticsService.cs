using GymMangment.BLL.Common;
using GymMangment.BLL.ViewModels;
using GymMangment.BLL.ViewModels.AnalyticsViewModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GymMangment.BLL.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<DashboardViewModel> GetDashboardDataAsync(CancellationToken ct = default);
        Task<Result<MemberAnalyticsViewModel>> GetMemberAnalyticsAsync(int memberId, int days = 180, CancellationToken ct = default);
        Task<Result<IEnumerable<ExerciseProgressPoint>>> GetMemberExerciseProgressAsync(int memberId, string exerciseName, int days = 180, CancellationToken ct = default);
    }
}