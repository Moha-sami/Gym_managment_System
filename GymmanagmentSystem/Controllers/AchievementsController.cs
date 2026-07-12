using GymManagment.DAL.Models;
using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.BadgeViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace GymmanagmentSystem.PL.Controllers
{
    [Authorize]
    public class AchievementsController : Controller
    {
        private readonly IBadgeService _badgeService;
        private readonly UserManager<AppUser> _userManager;

        public AchievementsController(IBadgeService badgeService, UserManager<AppUser> userManager)
        {
            _badgeService = badgeService;
            _userManager = userManager;
        }

        private async Task<int?> GetCurrentMemberIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.MemberId;
        }

        [Authorize(Roles = "Member")]
        public async Task<IActionResult> MyAchievements(CancellationToken ct)
        {
            var memberId = await GetCurrentMemberIdAsync();
            if (memberId == null) return Challenge();

            var result = await _badgeService.GetMemberAchievementsAsync(memberId.Value, ct);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction("Index", "Home");
            }

            return View(result.Data);
        }

        public async Task<IActionResult> Leaderboard(CancellationToken ct)
        {
            var result = await _badgeService.GetLeaderboardAsync(10, ct);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction("Index", "Home");
            }

            return View(result.Data);
        }
    }
}
