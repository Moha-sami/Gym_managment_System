using GymManagment.DAL.DbContext;
using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GymManagment.DAL.Repositories.Class
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity, new()
    {

        //Open connection
        private readonly GymDbcontext _context;
        private readonly DbSet<TEntity> _Set;//to access the table of the entity
        public GenericRepository(GymDbcontext context)
        {
            _context= context;
            _Set = _context.Set<TEntity>();
        }

        public async Task<int> AddAsync(TEntity Entity, CancellationToken ct)
        {
            _Set.Add(Entity);
            return await _context.SaveChangesAsync(ct);
        }

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> Predicate, CancellationToken ct = default)
        {
            return _Set.AnyAsync(Predicate, ct);
        }

        public async Task<int> DeleteAsync(TEntity Entity, CancellationToken ct)
        {
            _Set.Remove(Entity);
            return await _context.SaveChangesAsync(ct);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(bool tracking = false, CancellationToken ct = default)
        {
            IQueryable<TEntity> query = tracking ? _Set : _Set.AsNoTracking();
            return await query.ToListAsync();
        }

        public async Task<TEntity?> GetByIdAsync(int id, CancellationToken ct)=>
            await _Set.FindAsync( id , ct);


        public async Task<int> UpdateAsync(TEntity Entity, CancellationToken ct)
        {
            _Set.Update(Entity);
            return await _context.SaveChangesAsync(ct);
        }
    }
}
