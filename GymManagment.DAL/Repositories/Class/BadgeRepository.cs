using GymManagment.DAL.DbContext;
using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GymManagment.DAL.Repositories.Class
{
    public class BadgeRepository : GenericRepository<BadgeDefinition>, IBadgeRepository
    {
        private readonly GymDbcontext _context;

        public BadgeRepository(GymDbcontext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BadgeDefinition>> GetAllDefinitionsAsync(CancellationToken ct = default)
        {
            return await _context.BadgeDefinitions
                .AsNoTracking()
                .OrderBy(b => b.Category)
                .ThenBy(b => b.Tier)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<MemberBadge>> GetMemberBadgesAsync(int memberId, CancellationToken ct = default)
        {
            return await _context.MemberBadges
                .AsNoTracking()
                .Include(mb => mb.BadgeDefinition)
                .Include(mb => mb.AwardedByUser)
                .Where(mb => mb.MemberId == memberId)
                .OrderByDescending(mb => mb.EarnedAtUtc)
                .ToListAsync(ct);
        }

        public async Task<bool> HasBadgeAsync(int memberId, int badgeDefinitionId, CancellationToken ct = default)
        {
            return await _context.MemberBadges
                .AnyAsync(mb => mb.MemberId == memberId && mb.BadgeDefinitionId == badgeDefinitionId, ct);
        }

        public async Task<int> AwardBadgeAsync(MemberBadge memberBadge, CancellationToken ct = default)
        {
            await _context.MemberBadges.AddAsync(memberBadge, ct);
            return await _context.SaveChangesAsync(ct);
        }

        public async Task<IEnumerable<(int MemberId, string MemberName, string? MemberPhoto, int BadgeCount, string? LatestBadgeName, string? LatestBadgeIcon)>> GetLeaderboardAsync(int topN = 10, CancellationToken ct = default)
        {
            var memberBadgeCounts = await _context.MemberBadges
                .AsNoTracking()
                .Include(mb => mb.Member)
                .GroupBy(mb => new { mb.MemberId, mb.Member.Name, mb.Member.Photo })
                .Select(g => new
                {
                    g.Key.MemberId,
                    MemberName = g.Key.Name,
                    MemberPhoto = g.Key.Photo,
                    BadgeCount = g.Count()
                })
                .OrderByDescending(x => x.BadgeCount)
                .Take(topN)
                .ToListAsync(ct);

            if (!memberBadgeCounts.Any())
            {
                return Enumerable.Empty<(int MemberId, string MemberName, string? MemberPhoto, int BadgeCount, string? LatestBadgeName, string? LatestBadgeIcon)>();
            }

            var memberIds = memberBadgeCounts.Select(x => x.MemberId).ToList();

            var earnedBadges = await _context.MemberBadges
                .AsNoTracking()
                .Include(mb => mb.BadgeDefinition)
                .Where(mb => memberIds.Contains(mb.MemberId))
                .ToListAsync(ct);

            var latestBadgesLookup = earnedBadges
                .GroupBy(mb => mb.MemberId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(mb => mb.EarnedAtUtc).FirstOrDefault()?.BadgeDefinition
                );

            return memberBadgeCounts.Select(x => (
                x.MemberId,
                x.MemberName,
                x.MemberPhoto,
                x.BadgeCount,
                latestBadgesLookup.TryGetValue(x.MemberId, out var badge) ? badge?.Name : null,
                latestBadgesLookup.TryGetValue(x.MemberId, out var badge2) ? badge2?.IconPath : null
            ));
        }
    }
}
