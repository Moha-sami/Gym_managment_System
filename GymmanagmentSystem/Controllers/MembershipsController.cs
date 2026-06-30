using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.MembershipViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymmanagmentSystem.PL.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MembershipsController : Controller
    {
        private readonly IMembershipService _membershipService;

        public MembershipsController(IMembershipService membershipService)
        {
            _membershipService = membershipService;
        }

        // GET: Memberships/Index
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var result = await _membershipService.GetAllMembershipsAsync(ct);
            return View(result.Data);
        }

        // GET: Memberships/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var result = await _membershipService.GetMembershipByIdAsync(id, ct);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        // GET: Memberships/Create
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var result = await _membershipService.GetCreateFormDataAsync(ct);
            return View(result.Data);
        }

        // POST: Memberships/Create
        [HttpPost]
        public async Task<IActionResult> Create(CreateMembershipViewModel model, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
            {
                var formData = await _membershipService.GetCreateFormDataAsync(ct);
                model.Members = formData.Data!.Members;
                model.Plans = formData.Data!.Plans;
                return View(model);
            }

            var result = await _membershipService.CreateMembershipAsync(model, ct);

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"]
                = result.Succeeded ? "Membership created successfully!" : result.Error;

            if (!result.Succeeded)
            {
                var formData = await _membershipService.GetCreateFormDataAsync(ct);
                model.Members = formData.Data!.Members;
                model.Plans = formData.Data!.Plans;
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Memberships/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _membershipService.GetMembershipByIdAsync(id, ct);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        // POST: Memberships/DeleteConfirmed/5
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
        {
            var result = await _membershipService.DeleteMembershipAsync(id, ct);

            TempData[result.Succeeded ? "WarningMessage" : "ErrorMessage"]
                = result.Succeeded ? "Membership deleted successfully!" : result.Error;

            return RedirectToAction(nameof(Index));
        }
    }
}