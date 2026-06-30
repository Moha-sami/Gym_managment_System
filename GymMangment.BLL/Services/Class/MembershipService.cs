using AutoMapper;
using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Interfaces;
using GymMangment.BLL.Common;
using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.MembershipViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymMangment.BLL.Services.Class
{
    public class MembershipService : IMembershipService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MembershipService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<MembershipViewModel>>> GetAllMembershipsAsync(CancellationToken ct = default)
        {
            var memberships = await _unitOfWork.Memberships.GetAllAsync(
                false, ct,
                m => m.Member,
                m => m.Plans);

            var model = _mapper.Map<IEnumerable<MembershipViewModel>>(memberships);
            return Result<IEnumerable<MembershipViewModel>>.Success(model);
        }

        public async Task<Result<MembershipViewModel?>> GetMembershipByIdAsync(int id, CancellationToken ct = default)
        {
            var memberships = await _unitOfWork.Memberships.GetAllAsync(
                false, ct,
                m => m.Member,
                m => m.Plans);

            var membership = memberships.FirstOrDefault(m => m.Id == id);
            if (membership == null)
                return Result<MembershipViewModel?>.Failure("Membership not found");

            var model = _mapper.Map<MembershipViewModel>(membership);
            return Result<MembershipViewModel?>.Success(model);
        }

        public async Task<Result<CreateMembershipViewModel>> GetCreateFormDataAsync(CancellationToken ct = default)
        {
            var members = await _unitOfWork.Members.GetAllAsync(ct: ct);
            var plans = await _unitOfWork.Plans.GetAllAsync(ct: ct);

            var model = new CreateMembershipViewModel
            {
                Members = members.Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Name
                }),
                Plans = plans.Where(p => p.IsActive).Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.Name} ({p.DurationInDays} days - {p.Price} EGP)"
                })
            };

            return Result<CreateMembershipViewModel>.Success(model);
        }

        public async Task<Result> CreateMembershipAsync(CreateMembershipViewModel model, CancellationToken ct = default)
        {
            // Check if member already has an active membership
            var existing = await _unitOfWork.Memberships.GetAllAsync(
                false, ct,
                m => m.Member,
                m => m.Plans);

            var hasActive = existing.Any(m => m.MemberID == model.MemberID && m.IsActive);
            if (hasActive)
                return Result.Failure("This member already has an active membership.");

            // Set EndDate based on plan duration
            var plan = await _unitOfWork.Plans.GetByIdAsync(model.PlansID, ct);
            if (plan == null)
                return Result.Failure("Plan not found.");

            var membership = _mapper.Map<Membership>(model);
            membership.EndDate = DateTime.Now.AddDays(plan.DurationInDays);
            membership.CreatedAt = DateTime.Now;
            membership.UpdatedAt = DateTime.Now;

            var rows = await _unitOfWork.Memberships.AddAsync(membership, ct);
            return rows > 0
                ? Result.Success()
                : Result.Failure("Failed to create membership. Please try again.");
        }

        public async Task<Result> DeleteMembershipAsync(int id, CancellationToken ct = default)
        {
            var membership = await _unitOfWork.Memberships.GetByIdAsync(id, ct);
            if (membership == null)
                return Result.Failure("Membership not found");

            await _unitOfWork.Memberships.DeleteAsync(membership, ct);
            return Result.Success();
        }

        public async Task<Result<MyMembershipViewModel?>> GetMyMembershipAsync(int memberId, CancellationToken ct = default)
        {
            var memberships = await _unitOfWork.Memberships.GetAllAsync(
                false, ct,
                m => m.Member,
                m => m.Plans);

            var membership = memberships
                .Where(m => m.MemberID == memberId)
                .OrderByDescending(m => m.EndDate)
                .FirstOrDefault();

            if (membership == null)
                return Result<MyMembershipViewModel?>.Failure("You don't have an active membership yet.");

            var model = new MyMembershipViewModel
            {
                PlanName = membership.Plans.Name,
                PlanDescription = membership.Plans.Description,
                Price = membership.Plans.Price,
                StartDate = membership.CreatedAt,
                EndDate = membership.EndDate,
                IsActive = membership.IsActive,
                DaysRemaining = Math.Max(0, (membership.EndDate - DateTime.Now).Days)
            };

            return Result<MyMembershipViewModel?>.Success(model);
        }

        public async Task<Result> UpgradePlanAsync(int memberId, int newPlanId, CancellationToken ct = default)
        {
            var plan = await _unitOfWork.Plans.GetByIdAsync(newPlanId, ct);
            if (plan == null || !plan.IsActive)
                return Result.Failure("This plan is not available.");

            var memberships = await _unitOfWork.Memberships.GetAllAsync(ct: ct);
            var currentMembership = memberships
                .Where(m => m.MemberID == memberId)
                .OrderByDescending(m => m.EndDate)
                .FirstOrDefault();

            if (currentMembership != null && currentMembership.PlansID == newPlanId)
                return Result.Failure("You are already subscribed to this plan.");

            if (currentMembership != null && currentMembership.IsActive)
            {
                // Replace current active membership with new plan
                currentMembership.PlansID = newPlanId;
                currentMembership.EndDate = DateTime.Now.AddDays(plan.DurationInDays);
                currentMembership.UpdatedAt = DateTime.Now;
                await _unitOfWork.Memberships.UpdateAsync(currentMembership, ct);
            }
            else
            {
                // No active membership, create a new one
                var newMembership = new Membership
                {
                    MemberID = memberId,
                    PlansID = newPlanId,
                    EndDate = DateTime.Now.AddDays(plan.DurationInDays),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                await _unitOfWork.Memberships.AddAsync(newMembership, ct);
            }

            return Result.Success();
        }
    }
}