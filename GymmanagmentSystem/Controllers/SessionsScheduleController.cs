using GymManagment.DAL.Models;
using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.BookingViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GymmanagmentSystem.PL.Controllers
{
    [Authorize(Roles = "Admin,Manager,Member,Trainer")]
    public class SessionsScheduleController : Controller
    {
        private readonly IScheduleService _scheduleService;
        private readonly UserManager<AppUser> _userManager;

        public SessionsScheduleController(IScheduleService scheduleService, UserManager<AppUser> userManager)
        {
            _scheduleService = scheduleService;
            _userManager = userManager;
        }

        // GET: SessionsSchedule/Index
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var result = await _scheduleService.GetScheduleAsync(ct);
            return View(result.Data);
        }

        // GET: SessionsSchedule/Book/5    
        [HttpGet]
        public async Task<IActionResult> Book(int id, CancellationToken ct)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user?.MemberId == null)
            {
                TempData["ErrorMessage"] = "Your account is not linked to a member profile.";
                return RedirectToAction(nameof(Index));
            }

            var model = new CreateBookingViewModel
            {
                SessionId = id,
                MemberId = user.MemberId.Value
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Book(CreateBookingViewModel model, CancellationToken ct = default)
        {
            var result = await _scheduleService.BookSessionAsync(model.SessionId, model.MemberId, ct);

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"]
                = result.Succeeded ? "Session booked successfully!" : result.Error;

            return RedirectToAction(nameof(Index));
        }
    }
}
