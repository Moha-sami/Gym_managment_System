using GymManagment.DAL.Models;
using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.MembershipViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GymmanagmentSystem.PL.Controllers
{
   
    public class MembershipsController : Controller
    {
        private readonly IMembershipService _membershipService;
        private readonly UserManager<AppUser> _userManager;

        public MembershipsController(IMembershipService membershipService, UserManager<AppUser> userManager)
        {
            _membershipService = membershipService;
            _userManager = userManager;
        }
        [Authorize(Roles = "Admin")]
        // GET: Memberships/Index
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var result = await _membershipService.GetAllMembershipsAsync(ct);
            return View(result.Data);
        }
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        // GET: Memberships/Create
        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            var result = await _membershipService.GetCreateFormDataAsync(ct);
            return View(result.Data);
        }
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        // GET: Memberships/MyMembership
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> MyMembership(CancellationToken ct)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user?.MemberId == null)
            {
                TempData["ErrorMessage"] = "Your account is not linked to a member profile.";
                return RedirectToAction("Index", "Home");
            }

            var result = await _membershipService.GetMyMembershipAsync(user.MemberId.Value, ct);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction("Index", "Home");
            }

            return View(result.Data);
        }
        [Authorize(Roles = "Admin")]
        // POST: Memberships/DeleteConfirmed/5
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
        {
            var result = await _membershipService.DeleteMembershipAsync(id, ct);

            TempData[result.Succeeded ? "WarningMessage" : "ErrorMessage"]
                = result.Succeeded ? "Membership deleted successfully!" : result.Error;

            return RedirectToAction(nameof(Index));
        }

        //Upgrade Member Plan
        [HttpPost]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> UpgradePlan(int planId, CancellationToken ct)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user?.MemberId == null)
            {
                TempData["ErrorMessage"] = "Your account is not linked to a member profile.";
                return RedirectToAction("Index", "Home");
            }

            var result = await _membershipService.UpgradePlanAsync(user.MemberId.Value, planId, ct);

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"]
                = result.Succeeded ? "Your plan has been updated successfully!" : result.Error;

            return RedirectToAction(nameof(MyMembership));
        }
    }
}