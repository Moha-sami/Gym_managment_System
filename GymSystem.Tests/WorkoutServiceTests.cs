using AutoMapper;
using GymManagment.DAL.DbContext;
using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Class;
using GymMangment.BLL.Services.Class;
using GymMangment.BLL.ViewModels.WorkoutViewModels;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GymSystem.Tests
{
    public class WorkoutServiceTests : IDisposable
    {
        private readonly GymDbcontext _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly UnitOfWork _unitOfWork;
        private readonly WorkoutService _service;

        public WorkoutServiceTests()
        {
            var options = new DbContextOptionsBuilder<GymDbcontext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GymDbcontext(options);
            _context.Database.EnsureCreated();

            _mockMapper = new Mock<IMapper>();

            // Setup mapping from CreateWorkoutLogViewModel to WorkoutLog
            _mockMapper.Setup(m => m.Map<WorkoutLog>(It.IsAny<CreateWorkoutLogViewModel>()))
                .Returns((CreateWorkoutLogViewModel src) => new WorkoutLog
                {
                    Name = src.Name,
                    Date = src.Date,
                    Notes = src.Notes,
                    MemberId = src.MemberId,
                    Exercises = src.Exercises.Select(e => new WorkoutExerciseLog
                    {
                        ExerciseName = e.ExerciseName,
                        Sets = e.Sets.Select(s => new WorkoutSetLog
                        {
                            SetNumber = s.SetNumber,
                            Weight = s.Weight,
                            Reps = s.Reps
                        }).ToList()
                    }).ToList()
                });

            // Setup mapping from IEnumerable<WorkoutLog> to IEnumerable<WorkoutLogViewModel>
            _mockMapper.Setup(m => m.Map<IEnumerable<WorkoutLogViewModel>>(It.IsAny<IEnumerable<WorkoutLog>>()))
                .Returns((IEnumerable<WorkoutLog> src) => src.Select(w => new WorkoutLogViewModel
                {
                    Id = w.Id,
                    Name = w.Name,
                    Date = w.Date,
                    Notes = w.Notes,
                    MemberId = w.MemberId,
                    Exercises = w.Exercises.Select(e => new WorkoutExerciseViewModel
                    {
                        Id = e.Id,
                        ExerciseName = e.ExerciseName,
                        Sets = e.Sets.Select(s => new WorkoutSetViewModel
                        {
                            Id = s.Id,
                            SetNumber = s.SetNumber,
                            Weight = s.Weight,
                            Reps = s.Reps
                        }).ToList()
                    }).ToList()
                }));

            // Setup mapping from WorkoutLog to WorkoutLogViewModel
            _mockMapper.Setup(m => m.Map<WorkoutLogViewModel>(It.IsAny<WorkoutLog>()))
                .Returns((WorkoutLog w) => new WorkoutLogViewModel
                {
                    Id = w.Id,
                    Name = w.Name,
                    Date = w.Date,
                    Notes = w.Notes,
                    MemberId = w.MemberId,
                    Exercises = w.Exercises.Select(e => new WorkoutExerciseViewModel
                    {
                        Id = e.Id,
                        ExerciseName = e.ExerciseName,
                        Sets = e.Sets.Select(s => new WorkoutSetViewModel
                        {
                            Id = s.Id,
                            SetNumber = s.SetNumber,
                            Weight = s.Weight,
                            Reps = s.Reps
                        }).ToList()
                    }).ToList()
                });

            var mockSessionRepository = new SessionRepository(_context);
            _unitOfWork = new UnitOfWork(_context, mockSessionRepository);
            _service = new WorkoutService(_unitOfWork, _mockMapper.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task SaveWorkoutAsync_WithValidWorkout_SavesSuccessfully()
        {
            // Arrange
            var member = new Member
            {
                Id = 1,
                Name = "John Doe",
                Email = "john.doe@example.com",
                Phone = "1234567890",
                Address = new Address { Street = "Street", City = "City", BuildingNumber = 1 }
            };
            _context.Member.Add(member);
            await _context.SaveChangesAsync();

            var model = new CreateWorkoutLogViewModel
            {
                Name = "Leg Day",
                Date = DateTime.Today,
                Notes = "Feeling good",
                MemberId = 1,
                Exercises = new List<CreateWorkoutExerciseViewModel>
                {
                    new CreateWorkoutExerciseViewModel
                    {
                        ExerciseName = "Squats",
                        Sets = new List<CreateWorkoutSetViewModel>
                        {
                            new CreateWorkoutSetViewModel { SetNumber = 1, Weight = 100, Reps = 10 }
                        }
                    }
                }
            };

            // Act
            var result = await _service.SaveWorkoutAsync(model);

            // Assert
            Assert.True(result.Succeeded);
            var savedWorkouts = await _context.WorkoutLogs
                .Include(w => w.Exercises)
                .ThenInclude(e => e.Sets)
                .ToListAsync();

            Assert.Single(savedWorkouts);
            Assert.Equal("Leg Day", savedWorkouts[0].Name);
            Assert.Single(savedWorkouts[0].Exercises);
            Assert.Equal("Squats", savedWorkouts[0].Exercises.First().ExerciseName);
            Assert.Single(savedWorkouts[0].Exercises.First().Sets);
            Assert.Equal(100, savedWorkouts[0].Exercises.First().Sets.First().Weight);
        }

        [Fact]
        public async Task SaveWorkoutAsync_WithInvalidMember_ReturnsFailure()
        {
            // Arrange
            var model = new CreateWorkoutLogViewModel
            {
                Name = "Chest Day",
                Date = DateTime.Today,
                MemberId = 99, // Non-existent member ID
                Exercises = []
            };

            // Act
            var result = await _service.SaveWorkoutAsync(model);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Member not found.", result.Error);
        }

        [Fact]
        public async Task GetMemberWorkoutsAsync_ReturnsOnlyMemberWorkouts()
        {
            // Arrange
            var member1 = new Member { Id = 1, Name = "Member 1", Email = "m1@test.com", Phone = "1234567891", Address = new Address { Street = "Street", City = "City", BuildingNumber = 1 } };
            var member2 = new Member { Id = 2, Name = "Member 2", Email = "m2@test.com", Phone = "1234567892", Address = new Address { Street = "Street", City = "City", BuildingNumber = 1 } };
            _context.Member.AddRange(member1, member2);

            var workout1 = new WorkoutLog { Id = 1, MemberId = 1, Name = "M1 Workout", Date = DateTime.Today };
            var workout2 = new WorkoutLog { Id = 2, MemberId = 2, Name = "M2 Workout", Date = DateTime.Today };
            _context.WorkoutLogs.AddRange(workout1, workout2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetMemberWorkoutsAsync(1);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal("M1 Workout", result.Data.First().Name);
        }

        [Fact]
        public async Task DeleteWorkoutAsync_RemovesWorkoutAndNestedExercisesAndSets()
        {
            // Arrange
            var member = new Member { Id = 1, Name = "John", Email = "j@t.com", Phone = "1234567893", Address = new Address { Street = "Street", City = "City", BuildingNumber = 1 } };
            _context.Member.Add(member);

            var workout = new WorkoutLog
            {
                Id = 10,
                MemberId = 1,
                Name = "Full Body",
                Date = DateTime.Today,
                Exercises = new List<WorkoutExerciseLog>
                {
                    new WorkoutExerciseLog
                    {
                        Id = 1,
                        ExerciseName = "Deadlift",
                        Sets = new List<WorkoutSetLog>
                        {
                            new WorkoutSetLog { Id = 1, SetNumber = 1, Weight = 140, Reps = 5 }
                        }
                    }
                }
            };
            _context.WorkoutLogs.Add(workout);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteWorkoutAsync(10, 1);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Null(await _context.WorkoutLogs.FindAsync(10));
            Assert.Null(await _context.WorkoutExerciseLogs.FindAsync(1));
            Assert.Null(await _context.WorkoutSetLogs.FindAsync(1));
        }
    }
}
