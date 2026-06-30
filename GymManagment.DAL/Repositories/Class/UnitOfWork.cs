using GymManagment.DAL.DbContext;
using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Interfaces;

namespace GymManagment.DAL.Repositories.Class
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly GymDbcontext _context;

        private IGenericRepository<Member>? _members;
        private IGenericRepository<HealthRecord>? _healthRecords;
        private IGenericRepository<Plans>? _plans;
        private IGenericRepository<Session>? _sessions;
        private IGenericRepository<Category>? _categories;
        private IGenericRepository<Membership>? _memberships;
        private IGenericRepository<Trainer>? _trainers;
        private IGenericRepository<DeleteRequest>? _deleteRequests;
        private IGenericRepository<WeightProgressRecord>? _weightProgressRecords;
        private IBookingRepository? _bookings;

        public UnitOfWork(GymDbcontext context, ISessionRepository sessionRepository)
        {
            _context = context;
            SessionRepository = sessionRepository;
            
        }

        public IGenericRepository<Member> Members =>
            _members ??= new GenericRepository<Member>(_context);

        public IGenericRepository<HealthRecord> HealthRecords =>
            _healthRecords ??= new GenericRepository<HealthRecord>(_context);

        public IGenericRepository<Plans> Plans =>
            _plans ??= new GenericRepository<Plans>(_context);

        public IGenericRepository<Session> Sessions =>
            _sessions ??= new GenericRepository<Session>(_context);

        public IBookingRepository Bookings =>
         _bookings ??= new BookingRepository(_context);

        public IGenericRepository<Category> Categories =>
            _categories ??= new GenericRepository<Category>(_context);

        public IGenericRepository<Membership> Memberships =>
            _memberships ??= new GenericRepository<Membership>(_context);

        public IGenericRepository<Trainer> Trainers =>
            _trainers ??= new GenericRepository<Trainer>(_context);

        public IGenericRepository<DeleteRequest> DeleteRequests =>
            _deleteRequests ??= new GenericRepository<DeleteRequest>(_context);

        public IGenericRepository<WeightProgressRecord> WeightProgressRecords =>
            _weightProgressRecords ??= new GenericRepository<WeightProgressRecord>(_context);

        public ISessionRepository SessionRepository {  get;  }

        public IBookingRepository _Bookings =>
         _bookings ??= new BookingRepository(_context);

        public async Task<int> CompleteAsync(CancellationToken ct = default) =>
            await _context.SaveChangesAsync(ct);

        public void Dispose() => _context.Dispose();
    }
}
