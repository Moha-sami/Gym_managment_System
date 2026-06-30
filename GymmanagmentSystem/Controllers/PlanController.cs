using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.PlansViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymmanagmentSystem.PL.Controllers
{
    
    public class PlansController : Controller
    {
        private readonly IPlanServices _planService;

        public PlansController(IPlanServices planService)
        {
            _planService = planService;
        }

        // GET: Plans/Index
        [Authorize(Roles = "Admin,Member")]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var result = await _planService.GetAllPlansAsync(ct);
            return View(result.Data);
        }

        // GET: Plans/Details/5
        
        [HttpGet]
        [Authorize(Roles = "Admin,Member")]
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var result = await _planService.GetPlanByIdAsync(id, ct);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        // GET: Plans/EditPlan/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditPlan(int id, CancellationToken ct)
        {
            var result = await _planService.GetPlanByIdAsync(id, ct);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction(nameof(Index));
            }

            // Map PlanViewModel -> EditPlanViewModel for the form
            var editModel = new EditPlanViewModel
            {
                Id = result.Data!.Id,
                Name = result.Data.Name,
                Description = result.Data.Description,
                Price = result.Data.Price,
                DurationInDays = result.Data.DurationInDays
            };

            return View(editModel);
        }

        // POST: Plans/EditPlan/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditPlan(int id, EditPlanViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _planService.EditPlanAsync(id, model, ct);

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"]
                = result.Succeeded ? "Plan updated successfully!" : result.Error;

            return RedirectToAction(nameof(Index));
        }

        // POST: Plans/ToggleActivation/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleActivation(int id, CancellationToken ct)
        {
            var result = await _planService.ToggleActivationAsync(id, ct);

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"]
                = result.Succeeded ? "Plan status changed!" : result.Error;

            return RedirectToAction(nameof(Index));
        }
    }
}