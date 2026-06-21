using AutoMapper;
using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Interfaces;
using GymMangment.BLL.Common;
using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.PlansViewModels;

namespace GymMangment.BLL.Services.Class
{
    public class PlanService : IPlanServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PlanService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<PlanViewModel>>> GetAllPlansAsync(CancellationToken ct = default)
        {
            var plans = await _unitOfWork.Plans.GetAllAsync(ct: ct);
            var model = _mapper.Map<IEnumerable<PlanViewModel>>(plans);
            return Result<IEnumerable<PlanViewModel>>.Success(model);
        }

        public async Task<Result<PlanViewModel?>> GetPlanByIdAsync(int id, CancellationToken ct = default)
        {
            var plan = await _unitOfWork.Plans.GetByIdAsync(id, ct);
            if (plan == null)
                return Result<PlanViewModel?>.Failure("No plan found with this id");

            var model = _mapper.Map<PlanViewModel>(plan);
            return Result<PlanViewModel?>.Success(model);
        }

        public async Task<Result> EditPlanAsync(int id, EditPlanViewModel model, CancellationToken ct = default)
        {
            var plan = await _unitOfWork.Plans.GetByIdAsync(id, ct);
            if (plan == null)
                return Result.Failure("No plan found with this id");

            _mapper.Map(model, plan);
            await _unitOfWork.Plans.UpdateAsync(plan, ct);
            return Result.Success();
        }

        public async Task<Result> ToggleActivationAsync(int id, CancellationToken ct = default)
        {
            var plan = await _unitOfWork.Plans.GetByIdAsync(id, ct);
            if (plan == null)
                return Result.Failure("No plan found with this id");

            plan.IsActive = !plan.IsActive;
            plan.UpdatedAt = DateTime.Now;

            await _unitOfWork.Plans.UpdateAsync(plan, ct);
            return Result.Success();
        }
    }
}