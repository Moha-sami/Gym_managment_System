using System.Security.Claims;
using GymManagment.DAL.Models;
using GymManagment.DAL.Models.Enum;
using GymManagment.DAL.Repositories.Interfaces;
using GymMangment.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymmanagmentSystem.PL.Controllers
{
    [Authorize]
    public class DeleteRequestsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ImemberService _memberService;
        private readonly ITrainerService _trainerService;
        private readonly ISessionService _sessionService;

        public DeleteRequestsController(
            IUnitOfWork unitOfWork,
            ImemberService memberService,
            ITrainerService trainerService,
            ISessionService sessionService)
        {
            _unitOfWork = unitOfWork;
            _memberService = memberService;
            _trainerService = trainerService;
            _sessionService = sessionService;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var requests = await _unitOfWork.DeleteRequests.GetAllAsync(ct: ct);
            var model = requests
                .OrderBy(r => r.Status)
                .ThenByDescending(r => r.CreatedAt)
                .ToList();

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> SubmitRequest(DeleteTargetType targetType, int targetId, string? reason, CancellationToken ct)
        {
            var targetName = await GetTargetNameAsync(targetType, targetId, ct);
            if (targetName == null)
            {
                TempData["ErrorMessage"] = "The item you want to delete was not found.";
                return RedirectToTargetIndex(targetType);
            }

            var hasPendingRequest = await _unitOfWork.DeleteRequests.AnyAsync(
                r => r.TargetType == targetType
                     && r.TargetId == targetId
                     && r.Status == DeleteRequestStatus.Pending,
                ct);

            if (hasPendingRequest)
            {
                TempData["WarningMessage"] = "A pending delete request already exists for this item.";
                return RedirectToTargetIndex(targetType);
            }

            var request = new DeleteRequest
            {
                TargetType = targetType,
                TargetId = targetId,
                TargetName = targetName,
                Reason = reason,
                RequestedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                RequestedByName = User.Identity?.Name ?? "Manager",
                Status = DeleteRequestStatus.Pending,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _unitOfWork.DeleteRequests.AddAsync(request, ct);
            TempData["SuccessMessage"] = "Delete request sent to admin for approval.";
            return RedirectToTargetIndex(targetType);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id, string? adminNote, CancellationToken ct)
        {
            var request = await _unitOfWork.DeleteRequests.GetByIdAsync(id, ct);
            if (request == null)
            {
                TempData["ErrorMessage"] = "Delete request not found.";
                return RedirectToAction(nameof(Index));
            }

            if (request.Status != DeleteRequestStatus.Pending)
            {
                TempData["WarningMessage"] = "This delete request has already been reviewed.";
                return RedirectToAction(nameof(Index));
            }

            var deleteResult = request.TargetType switch
            {
                DeleteTargetType.Member => await _memberService.DeleteMemberAsync(request.TargetId, ct),
                DeleteTargetType.Trainer => await _trainerService.DeleteTrainerAsync(request.TargetId, ct),
                DeleteTargetType.Session => await _sessionService.DeleteSessionAsync(request.TargetId, ct),
                _ => GymMangment.BLL.Common.Result.Failure("Unsupported delete request type.")
            };

            if (!deleteResult.Succeeded)
            {
                TempData["ErrorMessage"] = deleteResult.Error;
                return RedirectToAction(nameof(Index));
            }

            MarkReviewed(request, DeleteRequestStatus.Approved, adminNote);
            await _unitOfWork.DeleteRequests.UpdateAsync(request, ct);

            TempData["WarningMessage"] = "Delete request approved and item deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id, string? adminNote, CancellationToken ct)
        {
            var request = await _unitOfWork.DeleteRequests.GetByIdAsync(id, ct);
            if (request == null)
            {
                TempData["ErrorMessage"] = "Delete request not found.";
                return RedirectToAction(nameof(Index));
            }

            if (request.Status != DeleteRequestStatus.Pending)
            {
                TempData["WarningMessage"] = "This delete request has already been reviewed.";
                return RedirectToAction(nameof(Index));
            }

            MarkReviewed(request, DeleteRequestStatus.Rejected, adminNote);
            await _unitOfWork.DeleteRequests.UpdateAsync(request, ct);

            TempData["SuccessMessage"] = "Delete request rejected.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<string?> GetTargetNameAsync(DeleteTargetType targetType, int targetId, CancellationToken ct)
        {
            return targetType switch
            {
                DeleteTargetType.Member => (await _unitOfWork.Members.GetByIdAsync(targetId, ct))?.Name,
                DeleteTargetType.Trainer => (await _unitOfWork.Trainers.GetByIdAsync(targetId, ct))?.Name,
                DeleteTargetType.Session => (await _unitOfWork.Sessions.GetByIdAsync(
                    targetId,
                    ct,
                    s => s.Category,
                    s => s.Trainer)) is { } session
                        ? $"{session.Category.CategoryName} session with {session.Trainer.Name}"
                        : null,
                _ => null
            };
        }

        private void MarkReviewed(DeleteRequest request, DeleteRequestStatus status, string? adminNote)
        {
            request.Status = status;
            request.AdminNote = adminNote;
            request.ReviewedAt = DateTime.Now;
            request.UpdatedAt = DateTime.Now;
            request.ReviewedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            request.ReviewedByName = User.Identity?.Name;
        }

        private IActionResult RedirectToTargetIndex(DeleteTargetType targetType)
        {
            return targetType switch
            {
                DeleteTargetType.Member => RedirectToAction("Index", "Members"),
                DeleteTargetType.Trainer => RedirectToAction("Index", "Trainers"),
                DeleteTargetType.Session => RedirectToAction("Index", "Sessions"),
                _ => RedirectToAction("Index", "Home")
            };
        }
    }
}
