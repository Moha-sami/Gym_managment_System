using GymMangment.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymmanagmentSystem.PL.Controllers
{
    [Authorize(Roles = "Admin,Manager,Member,Trainer")]
    public class BookingsController : Controller
    {
        private readonly IScheduleService _scheduleService;

        public BookingsController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        // GET: Bookings/Index
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var result = await _scheduleService.GetAllBookingsAsync(ct);
            return View(result.Data);
        }

        // GET: Bookings/MemberBookings/5
        [HttpGet]
        public async Task<IActionResult> MemberBookings(int id, CancellationToken ct)
        {
            var result = await _scheduleService.GetMemberBookingsAsync(id, ct);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        // GET: Bookings/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var bookings = await _scheduleService.GetAllBookingsAsync(ct);
            var booking = bookings.Data?.FirstOrDefault(b => b.Id == id);
            if (booking == null)
            {
                TempData["ErrorMessage"] = "Booking not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(booking);
        }

        // POST: Bookings/DeleteConfirmed/5
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
        {
            var result = await _scheduleService.CancelBookingAsync(id, ct);

            TempData[result.Succeeded ? "WarningMessage" : "ErrorMessage"]
                = result.Succeeded ? "Booking cancelled successfully!" : result.Error;

            return RedirectToAction(nameof(Index));
        }
    }
}
