using GymMangment.BLL.Common;
using GymMangment.BLL.ViewModels.TrainerViewModels;

namespace GymMangment.BLL.Services.Interfaces
{
    public interface ITrainerService
    {
        Task<Result<IEnumerable<TrainerViewModel>>> GetAllTrainersAsync(CancellationToken ct = default);
        Task<Result<TrainerViewModel?>> GetTrainerByIdAsync(int id, CancellationToken ct = default);
        Task<Result> CreateTrainerAsync(CreateTrainerViewModel model, CancellationToken ct = default);
        Task<Result<TrainerToUpdateViewModel?>> GetTrainerForEditAsync(int id, CancellationToken ct = default);
        Task<Result> UpdateTrainerAsync(int id, TrainerToUpdateViewModel model, CancellationToken ct = default);
        Task<Result> DeleteTrainerAsync(int id, CancellationToken ct = default);
    }
}