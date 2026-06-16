using GymMangment.BLL.Common;
using GymMangment.BLL.ViewModels.HealthRecordsViewModels;
using GymMangment.BLL.ViewModels.MemberViewModels;

namespace GymMangment.BLL.Services.Interfaces
{
    public interface ImemberService
    {
        Task<IEnumerable<MemberViewModel>> GetAllMembersAsync(CancellationToken ct = default);
        Task<Result> CreateMemberAsync(CreateMemberViewModel model, CancellationToken ct = default);
        Task<Result<MemberViewModel?>> GetMemberDetailsByIdAsync(int id, CancellationToken ct = default);
        Task<Result<HealthRecordViewModel?>> GetMemberHealthRecordAsync(int memberId, CancellationToken ct = default);
        Task<Result<MemberToUpdateViewModel?>> GetMemberToUpdateAsync(int memberId, CancellationToken ct = default);
        Task<Result?> UpdateMemberAsync(int id, MemberToUpdateViewModel model, CancellationToken ct = default);   
        Task<Result> DeleteMemberAsync(int memberId, CancellationToken ct = default);
    }
}