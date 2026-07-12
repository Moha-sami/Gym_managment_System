using GymManagment.DAL.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GymManagment.DAL.Repositories.Interfaces
{
    public interface IBadgeRepository : IGenericRepository<BadgeDefinition>
    {
        Task<IEnumerable<BadgeDefinition>> GetAllDefinitionsAsync(CancellationToken ct = default);
        Task<IEnumerable<MemberBadge>> GetMemberBadgesAsync(int memberId, CancellationToken ct = default);
        Task<bool> HasBadgeAsync(int memberId, int badgeDefinitionId, CancellationToken ct = default);
        Task<int> AwardBadgeAsync(MemberBadge memberBadge, CancellationToken ct = default);
        Task<IEnumerable<(int MemberId, string MemberName, string? MemberPhoto, int BadgeCount, string? LatestBadgeName, string? LatestBadgeIcon)>> GetLeaderboardAsync(int topN = 10, CancellationToken ct = default);
    }
}
