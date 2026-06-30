using GymManagment.DAL.Models;

namespace GymManagment.DAL.Repositories.Interfaces;

public interface IBookingRepository : IGenericRepository<Booking>
{
    //  Composite Key
    Task<Booking?> GetByIdsAsync(int memberId, int sessionId, CancellationToken ct = default);
}
