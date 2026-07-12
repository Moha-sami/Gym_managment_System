using GymManagment.DAL.Models;
using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.WorkoutViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GymmanagmentSystem.PL.Controllers
{
    [Authorize(Roles = "Member")]
    public class WorkoutsController : Controller
    {
        private readonly IWorkoutService _workoutService;
        private readonly IBadgeService _badgeService;
        private readonly UserManager<AppUser> _userManager;

        public WorkoutsController(IWorkoutService workoutService, IBadgeService badgeService, UserManager<AppUser> userManager)
        {
            _workoutService = workoutService;
            _badgeService = badgeService;
            _userManager = userManager;
        }

        private async Task<int?> GetCurrentMemberIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.MemberId;
        }

        public async Task<IActionResult> MyJournal(CancellationToken ct)
        {
            var memberId = await GetCurrentMemberIdAsync();
            if (memberId == null) return Challenge();

            var result = await _workoutService.GetMemberWorkoutsAsync(memberId.Value, ct);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction("Index", "Home");
            }

            return View(result.Data);
        }

        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var memberId = await GetCurrentMemberIdAsync();
            if (memberId == null) return Challenge();

            var result = await _workoutService.GetWorkoutDetailsAsync(id, memberId.Value, ct);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction(nameof(MyJournal));
            }

            return View(result.Data);
        }

        public async Task<IActionResult> Create()
        {
            var memberId = await GetCurrentMemberIdAsync();
            if (memberId == null) return Challenge();

            var model = new CreateWorkoutLogViewModel
            {
                MemberId = memberId.Value,
                Date = DateTime.Today
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateWorkoutLogViewModel model, CancellationToken ct)
        {
            var memberId = await GetCurrentMemberIdAsync();
            if (memberId == null) return Challenge();

            model.MemberId = memberId.Value;

            // 1. Clean up empty exercises and sets first, so we don't validate discarded fields
            model.Exercises = model.Exercises?
                .Where(e => !string.IsNullOrWhiteSpace(e.ExerciseName))
                .ToList() ?? [];

            foreach (var exercise in model.Exercises)
            {
                exercise.Sets = exercise.Sets?
                    .Where(s => s.Reps > 0 && s.Weight >= 0)
                    .ToList() ?? [];
            }

            // 2. Clear initial validation state and re-validate the cleaned-up model
            ModelState.Clear();
            TryValidateModel(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 3. Save via service (where nested set number indexing and timestamping takes place)
            var result = await _workoutService.SaveWorkoutAsync(model, ct);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Workout logged successfully!";
                
                // Evaluate achievements & badges
                try
                {
                    var newBadges = await _badgeService.EvaluateAndAwardBadgesAsync(memberId.Value, ct);
                    if (newBadges != null && newBadges.Any())
                    {
                        TempData["NewBadges"] = System.Text.Json.JsonSerializer.Serialize(newBadges);
                    }
                }
                catch { }

                return RedirectToAction(nameof(MyJournal));
            }

            ModelState.AddModelError("", result.Error ?? "An error occurred while saving the workout.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var memberId = await GetCurrentMemberIdAsync();
            if (memberId == null) return Challenge();

            var result = await _workoutService.DeleteWorkoutAsync(id, memberId.Value, ct);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Workout deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = result.Error;
            }

            return RedirectToAction(nameof(MyJournal));
        }
    }
}
