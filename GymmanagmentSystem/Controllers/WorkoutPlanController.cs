using GymManagment.DAL.Models;
using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.WorkoutPlanViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GymmanagmentSystem.PL.Controllers
{
    [Authorize(Roles = "Member")]
    public class WorkoutPlanController : Controller
    {
        private readonly IWorkoutPlanService _workoutPlanService;
        private readonly UserManager<AppUser> _userManager;

        public WorkoutPlanController(IWorkoutPlanService workoutPlanService, UserManager<AppUser> userManager)
        {
            _workoutPlanService = workoutPlanService;
            _userManager = userManager;
        }

        // GET: WorkoutPlan/MyPlan
        [HttpGet]
        public async Task<IActionResult> MyPlan()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return Challenge();

            var plan = await _workoutPlanService.GetActivePlanAsync(userEmail);
            if (plan == null)
            {
                return RedirectToAction(nameof(Generate));
            }

            return View(plan);
        }

        // GET: WorkoutPlan/Generate
        [HttpGet]
        public async Task<IActionResult> Generate()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return Challenge();

            var plan = await _workoutPlanService.GetActivePlanAsync(userEmail);
            ViewBag.HasActivePlan = plan != null;

            return View(new GeneratePlanRequestViewModel());
        }

        // POST: WorkoutPlan/Generate
        [HttpPost]
        public async Task<IActionResult> Generate(GeneratePlanRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return Challenge();

            try
            {
                await _workoutPlanService.GenerateAndSavePlanAsync(userEmail, model.Goal, model.Frequency, model.ExperienceLevel);
                TempData["SuccessMessage"] = "Your workout plan has been generated successfully!";
                return RedirectToAction(nameof(MyPlan));
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        // POST: WorkoutPlan/LogDay
        [HttpPost]
        public async Task<IActionResult> LogDay(int dayNumber)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail)) return Challenge();

            var success = await _workoutPlanService.LogPlanDayToJournalAsync(userEmail, dayNumber);
            if (success)
            {
                TempData["SuccessMessage"] = "Workout day logged to your journal! You can now adjust sets and weights.";
                return RedirectToAction("MyJournal", "Workouts");
            }

            TempData["ErrorMessage"] = "Failed to log workout day.";
            return RedirectToAction(nameof(MyPlan));
        }
    }
}
