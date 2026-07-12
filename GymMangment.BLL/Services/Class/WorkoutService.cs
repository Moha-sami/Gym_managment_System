using AutoMapper;
using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Interfaces;
using GymMangment.BLL.Common;
using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.WorkoutViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GymMangment.BLL.Services.Class
{
    public class WorkoutService : IWorkoutService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public WorkoutService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<WorkoutLogViewModel>>> GetMemberWorkoutsAsync(int memberId, CancellationToken ct = default)
        {
            var workouts = await _unitOfWork.WorkoutLogs.GetWorkoutsWithExercisesAndSetsAsync(memberId, ct);
            var model = _mapper.Map<IEnumerable<WorkoutLogViewModel>>(workouts);
            return Result<IEnumerable<WorkoutLogViewModel>>.Success(model);
        }

        public async Task<Result<WorkoutLogViewModel?>> GetWorkoutDetailsAsync(int id, int memberId, CancellationToken ct = default)
        {
            var workout = await _unitOfWork.WorkoutLogs.GetWorkoutWithExercisesAndSetsAsync(id, memberId, ct);
            if (workout == null)
            {
                return Result<WorkoutLogViewModel?>.Failure("Workout not found.");
            }

            var model = _mapper.Map<WorkoutLogViewModel>(workout);
            return Result<WorkoutLogViewModel?>.Success(model);
        }

        public async Task<Result> SaveWorkoutAsync(CreateWorkoutLogViewModel model, CancellationToken ct = default)
        {
            var memberExists = await _unitOfWork.Members.AnyAsync(m => m.Id == model.MemberId, ct);
            if (!memberExists)
            {
                return Result.Failure("Member not found.");
            }

            // Cleanup empty exercises & sets at BLL service layer
            model.Exercises = model.Exercises?
                .Where(e => !string.IsNullOrWhiteSpace(e.ExerciseName))
                .ToList() ?? [];

            foreach (var exercise in model.Exercises)
            {
                exercise.Sets = exercise.Sets?
                    .Where(s => s.Reps > 0 && s.Weight >= 0)
                    .ToList() ?? [];

                // Re-index sets sequentially
                for (int i = 0; i < exercise.Sets.Count; i++)
                {
                    exercise.Sets[i].SetNumber = i + 1;
                }
            }

            if (!model.Exercises.Any())
            {
                return Result.Failure("Please add at least one exercise with valid sets.");
            }

            var workout = _mapper.Map<WorkoutLog>(model);
            workout.CreatedAt = DateTime.UtcNow;
            workout.UpdatedAt = DateTime.UtcNow;

            foreach (var exercise in workout.Exercises)
            {
                exercise.CreatedAt = DateTime.UtcNow;
                exercise.UpdatedAt = DateTime.UtcNow;
                foreach (var set in exercise.Sets)
                {
                    set.CreatedAt = DateTime.UtcNow;
                    set.UpdatedAt = DateTime.UtcNow;
                }
            }

            var rows = await _unitOfWork.WorkoutLogs.AddAsync(workout, ct);
            return rows > 0
                ? Result.Success()
                : Result.Failure("Failed to save workout log.");
        }

        public async Task<Result> DeleteWorkoutAsync(int id, int memberId, CancellationToken ct = default)
        {
            var workout = await _unitOfWork.WorkoutLogs.GetWorkoutWithExercisesAndSetsAsync(id, memberId, ct);
            if (workout == null)
            {
                return Result.Failure("Workout not found.");
            }

            var rows = await _unitOfWork.WorkoutLogs.DeleteAsync(workout, ct);
            return rows > 0
                ? Result.Success()
                : Result.Failure("Failed to delete workout log.");
        }
    }
}
