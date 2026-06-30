using GymMangment.BLL.Common;
using GymMangment.BLL.ViewModels.MembershipViewModels;

namespace GymMangment.BLL.Services.Interfaces
{
    public interface IMembershipService
    {
        Task<Result<IEnumerable<MembershipViewModel>>> GetAllMembershipsAsync(CancellationToken ct = default);
        Task<Result<MembershipViewModel?>> GetMembershipByIdAsync(int id, CancellationToken ct = default);
        Task<Result<CreateMembershipViewModel>> GetCreateFormDataAsync(CancellationToken ct = default);
        Task<Result> CreateMembershipAsync(CreateMembershipViewModel model, CancellationToken ct = default);
        Task<Result> DeleteMembershipAsync(int id, CancellationToken ct = default);
        Task<Result> UpgradePlanAsync(int memberId, int newPlanId, CancellationToken ct = default);
        Task<Result<MyMembershipViewModel?>> GetMyMembershipAsync(int memberId, CancellationToken ct = default);
    }
}