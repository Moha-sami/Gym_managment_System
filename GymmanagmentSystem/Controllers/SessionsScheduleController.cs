using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.BookingViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymmanagmentSystem.PL.Controllers
{
    [Authorize(Roles = "Admin,Manager,Member,Trainer")]
    public class SessionsScheduleController : Controller
    {
        private readonly IScheduleService _scheduleService;

        public SessionsScheduleController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
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
            var result = await _scheduleService.GetBookingFormDataAsync(id, ct);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        // POST: SessionsSchedule/Book
        [HttpPost]
        public async Task<IActionResult> Book(CreateBookingViewModel model, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
            {
                var formData = await _scheduleService.GetBookingFormDataAsync(model.SessionId, ct);
                model.Members = formData.Data!.Members;
                return View(model);
            }

            var result = await _scheduleService.BookSessionAsync(model.SessionId, model.MemberId, ct);

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"]
                = result.Succeeded ? "Session booked successfully!" : result.Error;

            return RedirectToAction(nameof(Index));
        }
    }
}
