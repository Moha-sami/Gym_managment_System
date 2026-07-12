using GymManagment.DAL.DbContext;
using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GymManagment.DAL.Repositories.Class
{
    public class WorkoutLogRepository : GenericRepository<WorkoutLog>, IWorkoutLogRepository
    {
        private readonly GymDbcontext _context;

        public WorkoutLogRepository(GymDbcontext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WorkoutLog>> GetWorkoutsWithExercisesAndSetsAsync(int memberId, CancellationToken ct = default)
        {
            return await _context.WorkoutLogs
                .AsNoTracking()
                .Include(w => w.Exercises)
                    .ThenInclude(e => e.Sets)
                .Where(w => w.MemberId == memberId)
                .OrderByDescending(w => w.Date)
                .ToListAsync(ct);
        }

        public async Task<WorkoutLog?> GetWorkoutWithExercisesAndSetsAsync(int id, int memberId, CancellationToken ct = default)
        {
            return await _context.WorkoutLogs
                .Include(w => w.Exercises)
                    .ThenInclude(e => e.Sets)
                .FirstOrDefaultAsync(w => w.Id == id && w.MemberId == memberId, ct);
        }
    }
}
