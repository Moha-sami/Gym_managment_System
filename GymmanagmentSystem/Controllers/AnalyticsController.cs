using GymManagment.DAL.Models;
using GymMangment.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace GymmanagmentSystem.PL.Controllers
{
    [Authorize]
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly UserManager<AppUser> _userManager;

        public AnalyticsController(IAnalyticsService analyticsService, UserManager<AppUser> userManager)
        {
            _analyticsService = analyticsService;
            _userManager = userManager;
        }

        private async Task<int?> GetCurrentMemberIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.MemberId;
        }

        [Authorize(Roles = "Member")]
        public async Task<IActionResult> MyProgress(int days = 180, CancellationToken ct = default)
        {
            var memberId = await GetCurrentMemberIdAsync();
            if (memberId == null)
            {
                TempData["ErrorMessage"] = "You do not have a linked member profile.";
                return RedirectToAction("Index", "Home");
            }

            var result = await _analyticsService.GetMemberAnalyticsAsync(memberId.Value, days, ct);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Days = days;
            return View(result.Data);
        }

        [Authorize(Roles = "Admin,Manager,Trainer")]
        public async Task<IActionResult> MemberProgress(int id, int days = 180, CancellationToken ct = default)
        {
            var result = await _analyticsService.GetMemberAnalyticsAsync(id, days, ct);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction("Index", "Members");
            }

            ViewBag.Days = days;
            return View(result.Data);
        }

        // AJAX API
        [HttpGet]
        public async Task<IActionResult> GetChartData(int memberId, int days = 180, CancellationToken ct = default)
        {
            // Verify access: Member can only view their own; Admin/Manager/Trainer can view any
            if (User.IsInRole("Member"))
            {
                var currentMemberId = await GetCurrentMemberIdAsync();
                if (currentMemberId != memberId)
                {
                    return Forbid();
                }
            }

            var result = await _analyticsService.GetMemberAnalyticsAsync(memberId, days, ct);
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return Json(result.Data);
        }

        // AJAX API
        [HttpGet]
        public async Task<IActionResult> GetExerciseData(int memberId, string exerciseName, int days = 180, CancellationToken ct = default)
        {
            if (User.IsInRole("Member"))
            {
                var currentMemberId = await GetCurrentMemberIdAsync();
                if (currentMemberId != memberId)
                {
                    return Forbid();
                }
            }

            var result = await _analyticsService.GetMemberExerciseProgressAsync(memberId, exerciseName, days, ct);
            if (!result.Succeeded)
            {
                return BadRequest(result.Error);
            }

            return Json(result.Data);
        }
    }
}
