using GymManagment.DAL.Models;

namespace GymManagment.DAL.Repositories.Interfaces
{
    public interface IPlanRepository
    {
        Task<IEnumerable<Plans>> GetAllAsync(bool tracking = false , CancellationToken ct=default);
        Task<Plans?> GetByIdAsync(int id,CancellationToken ct);

        Task<int> AddAsync(Plans plan, CancellationToken ct);
        Task<int> DeleteAsync(Plans plan, CancellationToken ct);
        Task<int> UpdateAsync(Plans plan, CancellationToken ct);
    }
}
