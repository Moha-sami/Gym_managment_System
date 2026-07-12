using GymMangment.BLL.Common;
using GymMangment.BLL.ViewModels.BadgeViewModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GymMangment.BLL.Services.Interfaces
{
    public interface IBadgeService
    {
        /// <summary>
        /// Gets the full achievements page data for a member: earned badges, all definitions, and progress.
        /// </summary>
        Task<Result<AchievementsPageViewModel>> GetMemberAchievementsAsync(int memberId, CancellationToken ct = default);

        /// <summary>
        /// Gets the leaderboard: top N members ranked by badge count.
        /// </summary>
        Task<Result<IEnumerable<LeaderboardEntryViewModel>>> GetLeaderboardAsync(int topN = 10, CancellationToken ct = default);

        /// <summary>
        /// Evaluates all automatic badge criteria for a member and awards any newly earned badges.
        /// Returns the names of any newly awarded badges (for toast notifications).
        /// Called inline after triggering actions (save workout, attend session, etc.)
        /// </summary>
        Task<IEnumerable<string>> EvaluateAndAwardBadgesAsync(int memberId, CancellationToken ct = default);

        /// <summary>
        /// Admin manually awards a badge to a member.
        /// </summary>
        Task<Result> AwardManualBadgeAsync(int memberId, int badgeDefinitionId, string awardedByUserId, CancellationToken ct = default);

        /// <summary>
        /// Gets the data needed to render the admin award badge form (available manual badges).
        /// </summary>
        Task<Result<AwardBadgeViewModel>> GetAwardBadgeFormAsync(int memberId, CancellationToken ct = default);
    }
}
