using GymManagment.DAL.DbContext;
using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymManagment.DAL.Repositories.Class;

public class BookingRepository : GenericRepository<Booking>, IBookingRepository
{
    private readonly GymDbcontext _context;

    public BookingRepository(GymDbcontext context) : base(context)
    {
        _context = context;
    }

    public async Task<Booking?> GetByIdsAsync(int memberId, int sessionId, CancellationToken ct = default)
    {
        return await _context.Booking
            .FirstOrDefaultAsync(b => b.MemberId == memberId && b.SessionId == sessionId, ct);
    }
}
