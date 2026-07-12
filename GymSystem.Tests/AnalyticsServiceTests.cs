using GymManagment.DAL.DbContext;
using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Class;
using GymManagment.DAL.Repositories.Interfaces;
using GymMangment.BLL.Services.Class;
using GymMangment.BLL.ViewModels.AnalyticsViewModels;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GymSystem.Tests
{
    public class AnalyticsServiceTests : IDisposable
    {
        private readonly GymDbcontext _context;
        private readonly UnitOfWork _unitOfWork;
        private readonly AnalyticsService _service;

        public AnalyticsServiceTests()
        {
            var options = new DbContextOptionsBuilder<GymDbcontext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GymDbcontext(options);
            _context.Database.EnsureCreated();

            var mockSessionRepo = new Mock<ISessionRepository>();

            _unitOfWork = new UnitOfWork(_context, mockSessionRepo.Object);
            _service = new AnalyticsService(_unitOfWork);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetMemberAnalyticsAsync_ShouldCalculateCorrectVolumeAndFrequency()
        {
            // Arrange
            var member = new Member { Name = "Jane Doe", Email = "jane@example.com", Phone = "987654" };
            await _context.Member.AddAsync(member);

            var workout = new WorkoutLog
            {
                Name = "Leg Day",
                Date = DateTime.Now.Date,
                Member = member,
                Exercises = new List<WorkoutExerciseLog>
                {
                    new WorkoutExerciseLog
                    {
                        ExerciseName = "Squat",
                        Sets = new List<WorkoutSetLog>
                        {
                            new WorkoutSetLog { SetNumber = 1, Weight = 100, Reps = 5 }, // 500
                            new WorkoutSetLog { SetNumber = 2, Weight = 100, Reps = 5 }  // 500
                        }
                    }
                }
            };
            await _context.WorkoutLogs.AddAsync(workout);

            var weightRecord = new WeightProgressRecord
            {
                Weight = 70.5m,
                RecordedAt = DateTime.Now.Date,
                Member = member
            };
            await _context.WeightProgressRecords.AddAsync(weightRecord);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetMemberAnalyticsAsync(member.Id, 30);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            
            // Weight
            Assert.Single(result.Data.WeightRecords);
            Assert.Equal(70.5m, result.Data.WeightRecords.First().Weight);

            // Volume
            Assert.Single(result.Data.WorkoutVolume);
            Assert.Equal(1000, result.Data.WorkoutVolume.First().Volume);

            // Frequency
            Assert.Single(result.Data.WorkoutFrequency);
            Assert.Equal(1, result.Data.WorkoutFrequency.First().WorkoutCount);

            // Unique Exercises
            Assert.Single(result.Data.LoggedExercises);
            Assert.Equal("Squat", result.Data.LoggedExercises.First());
        }

        [Fact]
        public async Task GetMemberExerciseProgressAsync_ShouldEstimate1RMCorrectly()
        {
            // Arrange
            var member = new Member { Name = "Jane Doe", Email = "jane@example.com", Phone = "987654" };
            await _context.Member.AddAsync(member);

            var workout = new WorkoutLog
            {
                Name = "Chest Day",
                Date = DateTime.Now.Date,
                Member = member,
                Exercises = new List<WorkoutExerciseLog>
                {
                    new WorkoutExerciseLog
                    {
                        ExerciseName = "Bench Press",
                        Sets = new List<WorkoutSetLog>
                        {
                            // 100 kg for 5 reps: 1RM = 100 / (1.0278 - (0.0278 * 5)) = 100 / 0.8888 = 112.5 kg
                            new WorkoutSetLog { SetNumber = 1, Weight = 100, Reps = 5 }
                        }
                    }
                }
            };
            await _context.WorkoutLogs.AddAsync(workout);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetMemberExerciseProgressAsync(member.Id, "Bench Press", 30);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            var points = result.Data.ToList();
            Assert.Single(points);
            Assert.Equal(100.0, points[0].MaxWeight);
            Assert.Equal(112.5, points[0].Estimated1RM);
        }
    }
}
