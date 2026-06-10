using GymManagment.DAL.DbContext;
using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GymManagment.DAL.Repositories.Class
{
    public class PlanRepository : IPlanRepository
    {
        private readonly GymDbcontext DbContext;
        public PlanRepository(GymDbcontext DbContext)
        {
           this.DbContext = DbContext;
        }
        //Get All Plans
        public async Task<IEnumerable<Plans>> GetAllAsync(bool tracking = false, CancellationToken ct = default)
        {
           // if (tracking)
           // {
           //    await DbContext.Plans.ToListAsync();
           // }
           //await DbContext.Plans.AsNoTracking().ToListAsync();

            IQueryable<Plans> query = tracking ? DbContext.Plans : DbContext.Plans.AsNoTracking();

            return await query.ToListAsync(ct);
        }
        //Add Plan
        public async Task<int> AddAsync(Plans plan, CancellationToken ct)
        {
            DbContext.Plans.Add(plan);
            return await DbContext.SaveChangesAsync(ct);   
        }
        //Remove Plan
        public async Task<int> DeleteAsync(Plans plan, CancellationToken ct)
        {
            DbContext.Plans.Remove(plan);
            return await DbContext.SaveChangesAsync(ct);
        }

        //Get Plan By Id
        public async Task<Plans?> GetByIdAsync(int id, CancellationToken ct)
        {
            return await DbContext.Plans.FindAsync( id , ct);

        }
        //Update Plan
        public async Task<int> UpdateAsync(Plans plan, CancellationToken ct)
        {
            return await DbContext.SaveChangesAsync(ct);
        }
    }
}
