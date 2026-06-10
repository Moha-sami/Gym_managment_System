using GymMangment.BLL.ViewModels.MemberViewModels;

namespace GymMangment.BLL.Services.Interfaces
{
    public interface ImemberService
    {
        Task<IEnumerable<MemberViewModel>> GetAllMembersAsync(CancellationToken ct=default);
        Task<bool> CreateMemberAsync(CreateMemberViewModel memberViewModel, CancellationToken ct = default);
        
    }
}
