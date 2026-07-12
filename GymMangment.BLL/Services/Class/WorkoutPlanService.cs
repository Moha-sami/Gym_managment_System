using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Interfaces;
using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.WorkoutPlanViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GymMangment.BLL.Services.Class
{
    public class WorkoutPlanService : IWorkoutPlanService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WorkoutPlanService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<WorkoutPlanViewModel?> GetActivePlanAsync(string userEmail)
        {
            var member = await _unitOfWork.Members.FirstOrDefaultAsync(m => m.Email == userEmail);
            if (member == null) return null;

            var plan = await _unitOfWork.MemberWorkoutPlans.FirstOrDefaultAsync(p => p.MemberId == member.Id);
            if (plan == null) return null;

            var days = JsonSerializer.Deserialize<List<WorkoutPlanDayViewModel>>(plan.PlanJson) ?? new();

            return new WorkoutPlanViewModel
            {
                Id = plan.Id,
                Goal = plan.Goal,
                Frequency = plan.Frequency,
                ExperienceLevel = plan.ExperienceLevel,
                Days = days
            };
        }

        public async Task<WorkoutPlanViewModel> GenerateAndSavePlanAsync(string userEmail, string goal, int frequency, string experienceLevel)
        {
            var member = await _unitOfWork.Members.FirstOrDefaultAsync(m => m.Email == userEmail);
            if (member == null) throw new ArgumentException("Member not found.");

            // Generate template
            var days = GenerateTemplateDays(goal, frequency, experienceLevel);

            var planJson = JsonSerializer.Serialize(days);

            var existingPlan = await _unitOfWork.MemberWorkoutPlans.FirstOrDefaultAsync(p => p.MemberId == member.Id);
            if (existingPlan != null)
            {
                existingPlan.Goal = goal;
                existingPlan.Frequency = frequency;
                existingPlan.ExperienceLevel = experienceLevel;
                existingPlan.PlanJson = planJson;
                existingPlan.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.MemberWorkoutPlans.UpdateAsync(existingPlan, default);
            }
            else
            {
                var newPlan = new MemberWorkoutPlan
                {
                    MemberId = member.Id,
                    Goal = goal,
                    Frequency = frequency,
                    ExperienceLevel = experienceLevel,
                    PlanJson = planJson,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _unitOfWork.MemberWorkoutPlans.AddAsync(newPlan, default);
            }

            await _unitOfWork.CompleteAsync();

            return new WorkoutPlanViewModel
            {
                Goal = goal,
                Frequency = frequency,
                ExperienceLevel = experienceLevel,
                Days = days
            };
        }

        public async Task<bool> LogPlanDayToJournalAsync(string userEmail, int dayNumber)
        {
            var member = await _unitOfWork.Members.FirstOrDefaultAsync(m => m.Email == userEmail);
            if (member == null) return false;

            var plan = await _unitOfWork.MemberWorkoutPlans.FirstOrDefaultAsync(p => p.MemberId == member.Id);
            if (plan == null) return false;

            var days = JsonSerializer.Deserialize<List<WorkoutPlanDayViewModel>>(plan.PlanJson) ?? new();
            var targetDay = days.FirstOrDefault(d => d.DayNumber == dayNumber);
            if (targetDay == null) return false;

            // Log workout
            var log = new WorkoutLog
            {
                MemberId = member.Id,
                Name = $"{plan.Goal} - {targetDay.DayName}",
                Date = DateTime.Now,
                Notes = $"Logged automatically from active {plan.Goal} ({plan.ExperienceLevel}) workout plan.",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Exercises = targetDay.Exercises.Select(e => new WorkoutExerciseLog
                {
                    ExerciseName = e.ExerciseName,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Sets = Enumerable.Range(1, e.Sets).Select(s => new WorkoutSetLog
                    {
                        SetNumber = s,
                        Weight = 0,
                        Reps = e.Reps,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    }).ToList()
                }).ToList()
            };

            await _unitOfWork.WorkoutLogs.AddAsync(log, default);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        private List<WorkoutPlanDayViewModel> GenerateTemplateDays(string goal, int frequency, string experienceLevel)
        {
            var days = new List<WorkoutPlanDayViewModel>();

            if (goal.Equals("Build Muscle", StringComparison.OrdinalIgnoreCase))
            {
                if (frequency == 3)
                {
                    days.Add(new WorkoutPlanDayViewModel
                    {
                        DayNumber = 1,
                        DayName = "Full Body A (Chest/Back focus)",
                        Exercises = new()
                        {
                            new() { ExerciseName = "Barbell Bench Press", Sets = 4, Reps = 8 },
                            new() { ExerciseName = "Bent-Over Barbell Row", Sets = 4, Reps = 8 },
                            new() { ExerciseName = "Barbell Squats", Sets = 3, Reps = 10 },
                            new() { ExerciseName = "Dumbbell Lateral Raise", Sets = 3, Reps = 12 },
                            new() { ExerciseName = "Dumbbell Bicep Curls", Sets = 3, Reps = 12 },
                            new() { ExerciseName = "Abdominal Plank", Sets = 3, Reps = 60 }
                        }
                    });
                    days.Add(new WorkoutPlanDayViewModel
                    {
                        DayNumber = 2,
                        DayName = "Full Body B (Legs/Shoulders focus)",
                        Exercises = new()
                        {
                            new() { ExerciseName = "Romanian Deadlifts", Sets = 4, Reps = 8 },
                            new() { ExerciseName = "Seated Overhead Press", Sets = 4, Reps = 8 },
                            new() { ExerciseName = "Leg Press Machine", Sets = 3, Reps = 12 },
                            new() { ExerciseName = "Lat Pulldown", Sets = 3, Reps = 10 },
                            new() { ExerciseName = "Tricep Rope Pushdown", Sets = 3, Reps = 12 },
                            new() { ExerciseName = "Hanging Leg Raise", Sets = 3, Reps = 15 }
                        }
                    });
                    days.Add(new WorkoutPlanDayViewModel
                    {
                        DayNumber = 3,
                        DayName = "Full Body C (Arms/Accessories focus)",
                        Exercises = new()
                        {
                            new() { ExerciseName = "Incline Dumbbell Press", Sets = 3, Reps = 10 },
                            new() { ExerciseName = "One-Arm Dumbbell Row", Sets = 3, Reps = 10 },
                            new() { ExerciseName = "Bulgarian Split Squats", Sets = 3, Reps = 10 },
                            new() { ExerciseName = "Face Pulls", Sets = 3, Reps = 15 },
                            new() { ExerciseName = "Hammer Bicep Curls", Sets = 3, Reps = 12 },
                            new() { ExerciseName = "Bicycle Crunches", Sets = 3, Reps = 20 }
                        }
                    });
                }
                else if (frequency == 4)
                {
                    days.Add(new WorkoutPlanDayViewModel { DayNumber = 1, DayName = "Upper Body A", Exercises = new() { new() { ExerciseName = "Bench Press", Sets = 4, Reps = 8 }, new() { ExerciseName = "Lat Pulldown", Sets = 4, Reps = 10 }, new() { ExerciseName = "Shoulder Press", Sets = 3, Reps = 10 }, new() { ExerciseName = "Tricep Extension", Sets = 3, Reps = 12 } } });
                    days.Add(new WorkoutPlanDayViewModel { DayNumber = 2, DayName = "Lower Body A", Exercises = new() { new() { ExerciseName = "Barbell Squats", Sets = 4, Reps = 8 }, new() { ExerciseName = "Leg Curl", Sets = 3, Reps = 12 }, new() { ExerciseName = "Calf Raise", Sets = 4, Reps = 15 }, new() { ExerciseName = "Plank", Sets = 3, Reps = 60 } } });
                    days.Add(new WorkoutPlanDayViewModel { DayNumber = 3, DayName = "Upper Body B", Exercises = new() { new() { ExerciseName = "Incline Press", Sets = 3, Reps = 10 }, new() { ExerciseName = "Cable Row", Sets = 4, Reps = 10 }, new() { ExerciseName = "Lateral Raise", Sets = 3, Reps = 12 }, new() { ExerciseName = "Bicep Curls", Sets = 3, Reps = 12 } } });
                    days.Add(new WorkoutPlanDayViewModel { DayNumber = 4, DayName = "Lower Body B", Exercises = new() { new() { ExerciseName = "Deadlift", Sets = 4, Reps = 5 }, new() { ExerciseName = "Leg Press", Sets = 3, Reps = 12 }, new() { ExerciseName = "Leg Extension", Sets = 3, Reps = 15 }, new() { ExerciseName = "Crunches", Sets = 3, Reps = 20 } } });
                }
                else
                {
                    days.Add(new WorkoutPlanDayViewModel { DayNumber = 1, DayName = "Chest & Triceps", Exercises = new() { new() { ExerciseName = "Bench Press", Sets = 4, Reps = 8 }, new() { ExerciseName = "Incline Press", Sets = 3, Reps = 10 }, new() { ExerciseName = "Skull Crushers", Sets = 3, Reps = 12 }, new() { ExerciseName = "Tricep Pushdown", Sets = 3, Reps = 12 } } });
                    days.Add(new WorkoutPlanDayViewModel { DayNumber = 2, DayName = "Back & Biceps", Exercises = new() { new() { ExerciseName = "Barbell Row", Sets = 4, Reps = 8 }, new() { ExerciseName = "Lat Pulldown", Sets = 4, Reps = 10 }, new() { ExerciseName = "Barbell Curl", Sets = 3, Reps = 12 }, new() { ExerciseName = "Hammer Curl", Sets = 3, Reps = 12 } } });
                    days.Add(new WorkoutPlanDayViewModel { DayNumber = 3, DayName = "Legs & Abs", Exercises = new() { new() { ExerciseName = "Squats", Sets = 4, Reps = 10 }, new() { ExerciseName = "Romanian Deadlift", Sets = 4, Reps = 10 }, new() { ExerciseName = "Leg Extensions", Sets = 3, Reps = 15 }, new() { ExerciseName = "Hanging Leg Raise", Sets = 3, Reps = 15 } } });
                    days.Add(new WorkoutPlanDayViewModel { DayNumber = 4, DayName = "Shoulders & Arms", Exercises = new() { new() { ExerciseName = "Overhead Press", Sets = 4, Reps = 8 }, new() { ExerciseName = "Lateral Raise", Sets = 4, Reps = 12 }, new() { ExerciseName = "Tricep Dips", Sets = 3, Reps = 12 }, new() { ExerciseName = "Incline Bicep Curl", Sets = 3, Reps = 12 } } });
                    days.Add(new WorkoutPlanDayViewModel { DayNumber = 5, DayName = "Active Recovery & Cardio", Exercises = new() { new() { ExerciseName = "Treadmill Run", Sets = 1, Reps = 30 }, new() { ExerciseName = "Elliptical", Sets = 1, Reps = 30 }, new() { ExerciseName = "Plank", Sets = 3, Reps = 60 } } });
                }
            }
            else if (goal.Equals("Lose Weight", StringComparison.OrdinalIgnoreCase))
            {
                days.Add(new WorkoutPlanDayViewModel
                {
                    DayNumber = 1,
                    DayName = "Fat Burning Circuit A",
                    Exercises = new()
                    {
                        new() { ExerciseName = "Goblet Squats", Sets = 4, Reps = 15 },
                        new() { ExerciseName = "Push-Ups", Sets = 4, Reps = 12 },
                        new() { ExerciseName = "Kettlebell Swings", Sets = 4, Reps = 20 },
                        new() { ExerciseName = "Dumbbell Thrusters", Sets = 3, Reps = 15 },
                        new() { ExerciseName = "Mountain Climbers", Sets = 3, Reps = 40 },
                        new() { ExerciseName = "Jump Rope", Sets = 3, Reps = 60 }
                    }
                });
                days.Add(new WorkoutPlanDayViewModel
                {
                    DayNumber = 2,
                    DayName = "Cardio Intervals",
                    Exercises = new()
                    {
                        new() { ExerciseName = "Treadmill Incline Walk", Sets = 1, Reps = 15 },
                        new() { ExerciseName = "Treadmill Sprint Intervals", Sets = 6, Reps = 30 },
                        new() { ExerciseName = "Rowing Machine", Sets = 1, Reps = 10 },
                        new() { ExerciseName = "Battle Ropes", Sets = 4, Reps = 30 },
                        new() { ExerciseName = "Bicycle Crunches", Sets = 3, Reps = 25 }
                    }
                });
                days.Add(new WorkoutPlanDayViewModel
                {
                    DayNumber = 3,
                    DayName = "Fat Burning Circuit B",
                    Exercises = new()
                    {
                        new() { ExerciseName = "Bodyweight Lunges", Sets = 4, Reps = 16 },
                        new() { ExerciseName = "Lat Pulldowns", Sets = 4, Reps = 15 },
                        new() { ExerciseName = "Dumbbell Renegade Rows", Sets = 3, Reps = 12 },
                        new() { ExerciseName = "Burpees", Sets = 3, Reps = 10 },
                        new() { ExerciseName = "Russian Twists", Sets = 3, Reps = 30 },
                        new() { ExerciseName = "Plank Hold", Sets = 3, Reps = 45 }
                    }
                });

                if (frequency >= 4)
                {
                    days.Add(new WorkoutPlanDayViewModel { DayNumber = 4, DayName = "Lower Body HIIT", Exercises = new() { new() { ExerciseName = "Jump Squats", Sets = 4, Reps = 15 }, new() { ExerciseName = "Step Ups", Sets = 4, Reps = 16 }, new() { ExerciseName = "Leg Press", Sets = 3, Reps = 15 }, new() { ExerciseName = "Plank Jacks", Sets = 3, Reps = 30 } } });
                }
                if (frequency == 5)
                {
                    days.Add(new WorkoutPlanDayViewModel { DayNumber = 5, DayName = "Core & Endurance", Exercises = new() { new() { ExerciseName = "Elliptical Cardio", Sets = 1, Reps = 30 }, new() { ExerciseName = "Crunches", Sets = 3, Reps = 25 }, new() { ExerciseName = "Superman Pose", Sets = 3, Reps = 15 }, new() { ExerciseName = "Plank", Sets = 3, Reps = 60 } } });
                }
            }
            else
            {
                days.Add(new WorkoutPlanDayViewModel
                {
                    DayNumber = 1,
                    DayName = "VO2 Max Intervals",
                    Exercises = new()
                    {
                        new() { ExerciseName = "Treadmill Run (Moderate)", Sets = 1, Reps = 10 },
                        new() { ExerciseName = "Treadmill Sprint Intervals", Sets = 8, Reps = 60 },
                        new() { ExerciseName = "Elliptical HIIT Intervals", Sets = 1, Reps = 15 },
                        new() { ExerciseName = "Abdominal Plank", Sets = 3, Reps = 60 }
                    }
                });
                days.Add(new WorkoutPlanDayViewModel
                {
                    DayNumber = 2,
                    DayName = "Steady State Cardio",
                    Exercises = new()
                    {
                        new() { ExerciseName = "Stationary Cycling (Zone 2)", Sets = 1, Reps = 45 },
                        new() { ExerciseName = "Rowing Machine (Endurance)", Sets = 1, Reps = 20 },
                        new() { ExerciseName = "Hanging Knee Raise", Sets = 3, Reps = 15 }
                    }
                });
                days.Add(new WorkoutPlanDayViewModel
                {
                    DayNumber = 3,
                    DayName = "Functional Conditioning",
                    Exercises = new()
                    {
                        new() { ExerciseName = "Kettlebell Swings", Sets = 4, Reps = 25 },
                        new() { ExerciseName = "Battle Ropes Intervals", Sets = 5, Reps = 45 },
                        new() { ExerciseName = "Box Jumps", Sets = 3, Reps = 12 },
                        new() { ExerciseName = "Plank Hold", Sets = 3, Reps = 60 }
                    }
                });

                if (frequency >= 4)
                {
                    days.Add(new WorkoutPlanDayViewModel { DayNumber = 4, DayName = "Running Endurance", Exercises = new() { new() { ExerciseName = "Outdoor/Treadmill Run", Sets = 1, Reps = 40 }, new() { ExerciseName = "Flutter Kicks", Sets = 3, Reps = 30 } } });
                }
                if (frequency == 5)
                {
                    days.Add(new WorkoutPlanDayViewModel { DayNumber = 5, DayName = "Active Recovery & Core", Exercises = new() { new() { ExerciseName = "Stretching/Yoga", Sets = 1, Reps = 25 }, new() { ExerciseName = "Elliptical (Low intensity)", Sets = 1, Reps = 30 }, new() { ExerciseName = "Bicycle Crunches", Sets = 3, Reps = 30 } } });
                }
            }

            if (experienceLevel.Equals("Beginner", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var day in days)
                {
                    foreach (var exercise in day.Exercises)
                    {
                        exercise.Sets = Math.Max(2, exercise.Sets - 1);
                    }
                }
            }
            else if (experienceLevel.Equals("Advanced", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var day in days)
                {
                    foreach (var exercise in day.Exercises)
                    {
                        exercise.Sets++;
                    }
                }
            }

            return days;
        }
    }
}
