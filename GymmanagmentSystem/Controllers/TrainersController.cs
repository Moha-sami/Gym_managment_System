using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.TrainerViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GymmanagmentSystem.PL.Controllers
{
    public class TrainersController : Controller
    {
        private readonly ITrainerService _trainerService;

        public TrainersController(ITrainerService trainerService)
        {
            _trainerService = trainerService;
        }

        // GET: Trainers/Index
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var result = await _trainerService.GetAllTrainersAsync(ct);
            return View(result.Data);
        }

        // GET: Trainers/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken ct)
        {
            var result = await _trainerService.GetTrainerByIdAsync(id, ct);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        // GET: Trainers/Create
        [HttpGet]
        public IActionResult Create() => View();

        // POST: Trainers/Create
        [HttpPost]
        public async Task<IActionResult> Create(CreateTrainerViewModel model, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _trainerService.CreateTrainerAsync(model, ct);

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"]
                = result.Succeeded ? "Trainer created successfully!" : result.Error;

            return RedirectToAction(nameof(Index));
        }

        // GET: Trainers/EditTrainer/5
        [HttpGet]
        public async Task<IActionResult> EditTrainer(int id, CancellationToken ct)
        {
            var result = await _trainerService.GetTrainerForEditAsync(id, ct);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        // POST: Trainers/EditTrainer/5
        [HttpPost]
        public async Task<IActionResult> EditTrainer(int id, TrainerToUpdateViewModel model, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _trainerService.UpdateTrainerAsync(id, model, ct);

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"]
                = result.Succeeded ? "Trainer updated successfully!" : result.Error;

            return RedirectToAction(nameof(Index));
        }

        // GET: Trainers/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _trainerService.GetTrainerByIdAsync(id, ct);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Error;
                return RedirectToAction(nameof(Index));
            }
            return View(result.Data);
        }

        // POST: Trainers/DeleteConfirmed/5
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
        {
            var result = await _trainerService.DeleteTrainerAsync(id, ct);

            TempData[result.Succeeded ? "WarningMessage" : "ErrorMessage"]
                = result.Succeeded ? "Trainer deleted successfully!" : result.Error;

            return RedirectToAction(nameof(Index));
        }
    }
}