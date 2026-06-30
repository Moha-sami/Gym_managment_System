using GymManagment.DAL.Models;
using GymMangment.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GymmanagmentSystem.PL.Controllers
{
    [Authorize(Roles = "Admin,Manager,Member,Trainer")]
    public class BookingsController : Controller
    {
        private readonly IScheduleService _scheduleService;
        private readonly UserManager<AppUser> _userManager;

        public BookingsController(IScheduleService scheduleService, UserManager<AppUser> userManager)
        {
            _scheduleService = scheduleService;
            _userManager = userManager;
        }

        // GET: Bookings/Index
        // GET: Bookings/Index
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var result = await _scheduleService.GetAllBookingsAsync(ct);
            return View(result.Data);
        }
        // GET: Bookings/MyBookings
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> MyBookings(CancellationToken ct)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user?.MemberId == null)
            {
                TempData["ErrorMessage"] = "Your account is not linked to a member profile.";
                return RedirectToAction("Index", "Home");
            }

            var result = await _scheduleService.GetMemberBookingsAsync(user.MemberId.Value, ct);
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
