using AutoMapper;
using GymManagment.DAL.Models;
using GymManagment.DAL.Models.Enum;
using GymManagment.DAL.Repositories.Interfaces;
using GymMangment.BLL.Common;
using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.BadgeViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GymMangment.BLL.Services.Class
{
    public class BadgeService : IBadgeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BadgeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<AchievementsPageViewModel>> GetMemberAchievementsAsync(int memberId, CancellationToken ct = default)
        {
            var member = await _unitOfWork.Members.GetByIdAsync(memberId, ct);
            if (member == null)
                return Result<AchievementsPageViewModel>.Failure("Member not found.");

            // Evaluate and catch up on any auto-awardable badges that haven't been recorded yet
            await EvaluateAndAwardBadgesAsync(memberId, ct);

            var allDefinitions = await _unitOfWork.Badges.GetAllDefinitionsAsync(ct);
            var earnedBadges = await _unitOfWork.Badges.GetMemberBadgesAsync(memberId, ct);
            var progress = await ComputeCategoryProgressAsync(memberId, ct);

            var vm = new AchievementsPageViewModel
            {
                MemberName = member.Name,
                TotalBadgeCount = earnedBadges.Count(),
                EarnedBadges = earnedBadges.Select(mb => new MemberBadgeViewModel
                {
                    Id = mb.Id,
                    BadgeDefinitionId = mb.BadgeDefinitionId,
                    BadgeName = mb.BadgeDefinition.Name,
                    BadgeDescription = mb.BadgeDefinition.Description,
                    BadgeIconPath = mb.BadgeDefinition.IconPath,
                    BadgeTier = mb.BadgeDefinition.Tier.ToString(),
                    BadgeCategory = mb.BadgeDefinition.Category.ToString(),
                    EarnedAtUtc = mb.EarnedAtUtc,
                    AwardedByUserName = mb.AwardedByUser?.UserName
                }).ToList(),
                AllDefinitions = allDefinitions.Select(bd => new BadgeDefinitionViewModel
                {
                    Id = bd.Id,
                    Name = bd.Name,
                    Description = bd.Description,
                    IconPath = bd.IconPath,
                    Category = bd.Category.ToString(),
                    Tier = bd.Tier.ToString(),
                    Threshold = bd.Threshold,
                    IsAutomatic = bd.IsAutomatic
                }).ToList(),
                CategoryProgress = progress
            };

            return Result<AchievementsPageViewModel>.Success(vm);
        }

        public async Task<Result<IEnumerable<LeaderboardEntryViewModel>>> GetLeaderboardAsync(int topN = 10, CancellationToken ct = default)
        {
            var entries = await _unitOfWork.Badges.GetLeaderboardAsync(topN, ct);
            int rank = 1;
            var viewModels = entries.Select(e => new LeaderboardEntryViewModel
            {
                Rank = rank++,
                MemberId = e.MemberId,
                MemberName = e.MemberName,
                MemberPhoto = e.MemberPhoto,
                BadgeCount = e.BadgeCount,
                LatestBadgeName = e.LatestBadgeName,
                LatestBadgeIconPath = e.LatestBadgeIcon
            }).ToList();

            return Result<IEnumerable<LeaderboardEntryViewModel>>.Success(viewModels);
        }

        public async Task<IEnumerable<string>> EvaluateAndAwardBadgesAsync(int memberId, CancellationToken ct = default)
        {
            var newlyAwarded = new List<string>();

            var allDefinitions = await _unitOfWork.Badges.GetAllDefinitionsAsync(ct);
            var autoDefinitions = allDefinitions.Where(d => d.IsAutomatic && d.Threshold.HasValue);

            var progress = await ComputeCategoryProgressAsync(memberId, ct);

            foreach (var badge in autoDefinitions)
            {
                // Skip if already earned
                if (await _unitOfWork.Badges.HasBadgeAsync(memberId, badge.Id, ct))
                    continue;

                var categoryKey = badge.Category.ToString();
                if (!progress.ContainsKey(categoryKey))
                    continue;

                var currentValue = progress[categoryKey];

                if (currentValue >= badge.Threshold!.Value)
                {
                    var memberBadge = new MemberBadge
                    {
                        MemberId = memberId,
                        BadgeDefinitionId = badge.Id,
                        EarnedAtUtc = DateTime.UtcNow,
                        AwardedByUserId = null,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.Badges.AwardBadgeAsync(memberBadge, ct);
                    newlyAwarded.Add(badge.Name);
                }
            }

            return newlyAwarded;
        }

        public async Task<Result> AwardManualBadgeAsync(int memberId, int badgeDefinitionId, string awardedByUserId, CancellationToken ct = default)
        {
            var memberExists = await _unitOfWork.Members.AnyAsync(m => m.Id == memberId, ct);
            if (!memberExists)
                return Result.Failure("Member not found.");

            var badge = await _unitOfWork.Badges.GetByIdAsync(badgeDefinitionId, ct);
            if (badge == null)
                return Result.Failure("Badge definition not found.");

            if (await _unitOfWork.Badges.HasBadgeAsync(memberId, badgeDefinitionId, ct))
                return Result.Failure("Member already has this badge.");

            var memberBadge = new MemberBadge
            {
                MemberId = memberId,
                BadgeDefinitionId = badgeDefinitionId,
                EarnedAtUtc = DateTime.UtcNow,
                AwardedByUserId = awardedByUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var rows = await _unitOfWork.Badges.AwardBadgeAsync(memberBadge, ct);
            return rows > 0
                ? Result.Success()
                : Result.Failure("Failed to award badge.");
        }

        public async Task<Result<AwardBadgeViewModel>> GetAwardBadgeFormAsync(int memberId, CancellationToken ct = default)
        {
            var member = await _unitOfWork.Members.GetByIdAsync(memberId, ct);
            if (member == null)
                return Result<AwardBadgeViewModel>.Failure("Member not found.");

            var allDefinitions = await _unitOfWork.Badges.GetAllDefinitionsAsync(ct);
            var manualBadges = allDefinitions.Where(d => !d.IsAutomatic);

            // Also get earned badges to filter them out
            var earnedBadgeIds = (await _unitOfWork.Badges.GetMemberBadgesAsync(memberId, ct))
                .Select(mb => mb.BadgeDefinitionId)
                .ToHashSet();

            var available = manualBadges
                .Where(d => !earnedBadgeIds.Contains(d.Id))
                .Select(d => new BadgeDefinitionViewModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    IconPath = d.IconPath,
                    Category = d.Category.ToString(),
                    Tier = d.Tier.ToString(),
                    Threshold = d.Threshold,
                    IsAutomatic = d.IsAutomatic
                }).ToList();

            return Result<AwardBadgeViewModel>.Success(new AwardBadgeViewModel
            {
                MemberId = memberId,
                MemberName = member.Name,
                AvailableBadges = available
            });
        }

        #region Private Helpers — Progress Computation

        private async Task<Dictionary<string, int>> ComputeCategoryProgressAsync(int memberId, CancellationToken ct)
        {
            var progress = new Dictionary<string, int>();

            // WorkoutCount — total workouts logged
            var workouts = await _unitOfWork.WorkoutLogs.GetWorkoutsWithExercisesAndSetsAsync(memberId, ct);
            var workoutList = workouts.ToList();
            progress[BadgeCategory.WorkoutCount.ToString()] = workoutList.Count;

            // TotalVolume — sum of (weight * reps) across all sets
            var totalVolume = workoutList
                .SelectMany(w => w.Exercises)
                .SelectMany(e => e.Sets)
                .Sum(s => (int)(s.Weight * s.Reps));
            progress[BadgeCategory.TotalVolume.ToString()] = totalVolume;

            // SessionAttendance — bookings where IsAttended == true
            var allBookings = await _unitOfWork.Bookings.GetAllAsync(false, ct);
            var memberBookings = allBookings.Where(b => b.MemberId == memberId).ToList();
            progress[BadgeCategory.SessionAttendance.ToString()] = memberBookings.Count(b => b.IsAttended);

            // BookingCount — total bookings
            progress[BadgeCategory.BookingCount.ToString()] = memberBookings.Count;

            // PersonalRecord — max weight lifted in any single set
            var maxWeight = workoutList
                .SelectMany(w => w.Exercises)
                .SelectMany(e => e.Sets)
                .DefaultIfEmpty()
                .Max(s => s != null ? (int)s.Weight : 0);
            progress[BadgeCategory.PersonalRecord.ToString()] = maxWeight;

            // ConsistencyStreak — consecutive days with logged workouts (current streak)
            var streak = ComputeCurrentStreak(workoutList.Select(w => w.Date).Distinct().OrderByDescending(d => d));
            progress[BadgeCategory.ConsistencyStreak.ToString()] = streak;

            // WeightProgress — absolute weight change from first to latest record
            var weightRecords = await _unitOfWork.WeightProgressRecords.GetAllAsync(false, ct);
            var memberWeightRecords = weightRecords
                .Where(r => r.MemberId == memberId)
                .OrderBy(r => r.RecordedAt)
                .ToList();
            if (memberWeightRecords.Count >= 2)
            {
                var weightChange = Math.Abs(memberWeightRecords.Last().Weight - memberWeightRecords.First().Weight);
                progress[BadgeCategory.WeightProgress.ToString()] = (int)weightChange;
            }
            else
            {
                progress[BadgeCategory.WeightProgress.ToString()] = 0;
            }

            return progress;
        }

        private static int ComputeCurrentStreak(IEnumerable<DateTime> sortedDatesDescending)
        {
            var dates = sortedDatesDescending.Select(d => d.Date).ToList();
            if (dates.Count == 0) return 0;

            // Only count streak if most recent workout is today or yesterday
            var today = DateTime.UtcNow.Date;
            if ((today - dates[0]).TotalDays > 1) return 0;

            int streak = 1;
            for (int i = 1; i < dates.Count; i++)
            {
                if ((dates[i - 1] - dates[i]).TotalDays == 1)
                    streak++;
                else
                    break;
            }

            return streak;
        }

        #endregion
    }
}
