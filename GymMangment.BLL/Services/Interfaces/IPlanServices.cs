using GymMangment.BLL.Common;
using GymMangment.BLL.ViewModels.PlansViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymMangment.BLL.Services.Interfaces
{
    public interface IPlanServices
    {
        Task<Result<IEnumerable<PlanViewModel>>> GetAllPlansAsync(CancellationToken ct = default);
        Task<Result<PlanViewModel?>> GetPlanByIdAsync(int id, CancellationToken ct = default);
        Task<Result> EditPlanAsync(int id, EditPlanViewModel model, CancellationToken ct = default);
        Task<Result> ToggleActivationAsync(int id, CancellationToken ct = default);
    }
}
