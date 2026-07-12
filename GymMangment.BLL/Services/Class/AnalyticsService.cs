using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Interfaces;
using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels;
using GymMangment.BLL.ViewModels.AnalyticsViewModels;
using GymMangment.BLL.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GymMangment.BLL.Services.Class
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AnalyticsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync(CancellationToken ct = default)
        {
            var members = await _unitOfWork.Members.GetAllAsync(ct: ct);
            var trainers = await _unitOfWork.Trainers.GetAllAsync(ct: ct);
            var sessions = await _unitOfWork.Sessions.GetAllAsync(ct: ct);
            var memberships = await _unitOfWork.Memberships.GetAllAsync(ct: ct);

            var now = DateTime.Now;

            return new DashboardViewModel
            {
                TotalMembers = members.Count(),
                ActiveMembers = memberships.Count(m => m.IsActive),
                TotalTrainers = trainers.Count(),
                UpcomingSessions = sessions.Count(s => s.StartDate > now),
                OngoingSessions = sessions.Count(s => s.StartDate <= now && s.EndDate >= now),
                CompletedSessions = sessions.Count(s => s.EndDate < now)
            };
        }

        public async Task<Result<MemberAnalyticsViewModel>> GetMemberAnalyticsAsync(int memberId, int days = 180, CancellationToken ct = default)
        {
            var member = await _unitOfWork.Members.GetByIdAsync(memberId, ct);
            if (member == null)
                return Result<MemberAnalyticsViewModel>.Failure("Member not found.");

            var cutoffDate = DateTime.Now.Date.AddDays(-days);

            // 1. Weight Records
            var allWeightRecords = await _unitOfWork.WeightProgressRecords.GetAllAsync(ct: ct);
            var weightRecords = allWeightRecords
                .Where(r => r.MemberId == memberId && r.RecordedAt >= cutoffDate)
                .OrderBy(r => r.RecordedAt)
                .Select(r => new WeightRecordPoint
                {
                    DateLabel = r.RecordedAt.ToLocalTime().ToString("MMM dd, yyyy"),
                    Weight = r.Weight
                })
                .ToList();

            // 2. Workouts & Volume
            var workouts = await _unitOfWork.WorkoutLogs.GetWorkoutsWithExercisesAndSetsAsync(memberId, ct);
            var workoutList = workouts
                .Where(w => w.Date >= cutoffDate)
                .OrderBy(w => w.Date)
                .ToList();

            // Volume per workout
            var volumePoints = workoutList.Select(w => new VolumePoint
            {
                DateLabel = w.Date.ToLocalTime().ToString("MMM dd"),
                Volume = w.Exercises
                    .SelectMany(e => e.Sets)
                    .Sum(s => (int)(s.Weight * s.Reps))
            }).ToList();

            // Workout Frequency (Group by Month or Week depending on total days)
            // For 30d, group by week. For longer, group by Month.
            var frequencyPoints = new List<FrequencyPoint>();
            if (days <= 30)
            {
                // Group by Week (using start of week date)
                var groupedByWeek = workoutList
                    .GroupBy(w => {
                        var diff = (7 + (w.Date.Date.DayOfWeek - DayOfWeek.Monday)) % 7;
                        return w.Date.Date.AddDays(-1 * diff).ToString("MMM dd");
                    })
                    .OrderBy(g => DateTime.Parse(g.Key));

                foreach (var group in groupedByWeek)
                {
                    frequencyPoints.Add(new FrequencyPoint
                    {
                        PeriodLabel = $"Week of {group.Key}",
                        WorkoutCount = group.Count()
                    });
                }
            }
            else
            {
                // Group by Month
                var groupedByMonth = workoutList
                    .GroupBy(w => w.Date.ToString("MMMM yyyy"))
                    .OrderBy(g => DateTime.ParseExact(g.Key, "MMMM yyyy", System.Globalization.CultureInfo.InvariantCulture));

                foreach (var group in groupedByMonth)
                {
                    frequencyPoints.Add(new FrequencyPoint
                    {
                        PeriodLabel = group.Key,
                        WorkoutCount = group.Count()
                    });
                }
            }

            // 3. Unique Logged Exercises
            var allWorkouts = await _unitOfWork.WorkoutLogs.GetWorkoutsWithExercisesAndSetsAsync(memberId, ct);
            var exercisesLogged = allWorkouts
                .SelectMany(w => w.Exercises)
                .Select(e => e.ExerciseName)
                .Distinct()
                .OrderBy(e => e)
                .ToList();

            var vm = new MemberAnalyticsViewModel
            {
                MemberId = memberId,
                MemberName = member.Name,
                WeightRecords = weightRecords,
                WorkoutVolume = volumePoints,
                WorkoutFrequency = frequencyPoints,
                LoggedExercises = exercisesLogged
            };

            return Result<MemberAnalyticsViewModel>.Success(vm);
        }

        public async Task<Result<IEnumerable<ExerciseProgressPoint>>> GetMemberExerciseProgressAsync(int memberId, string exerciseName, int days = 180, CancellationToken ct = default)
        {
            var cutoffDate = DateTime.Now.Date.AddDays(-days);
            var workouts = await _unitOfWork.WorkoutLogs.GetWorkoutsWithExercisesAndSetsAsync(memberId, ct);

            var exercisePoints = workouts
                .Where(w => w.Date >= cutoffDate)
                .OrderBy(w => w.Date)
                .Select(w => {
                    var matchingSets = w.Exercises
                        .Where(e => string.Equals(e.ExerciseName, exerciseName, StringComparison.OrdinalIgnoreCase))
                        .SelectMany(e => e.Sets)
                        .ToList();

                    if (!matchingSets.Any())
                        return null;

                    var maxWeight = matchingSets.Max(s => (double)s.Weight);
                    
                    // Brzycki formula for 1RM: Weight / (1.0278 - (0.0278 * Reps))
                    // If reps == 1, estimated 1RM is just the weight.
                    var max1RM = matchingSets.Max(s => s.Reps == 1 
                        ? (double)s.Weight 
                        : (double)s.Weight / (1.0278 - (0.0278 * s.Reps)));

                    return new ExerciseProgressPoint
                    {
                        DateLabel = w.Date.ToLocalTime().ToString("MMM dd"),
                        MaxWeight = Math.Round(maxWeight, 1),
                        Estimated1RM = Math.Round(max1RM, 1)
                    };
                })
                .Where(p => p != null)
                .Select(p => p!)
                .ToList();

            return Result<IEnumerable<ExerciseProgressPoint>>.Success(exercisePoints);
        }
    }
}