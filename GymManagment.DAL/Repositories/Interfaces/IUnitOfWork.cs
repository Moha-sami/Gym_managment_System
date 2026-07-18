using GymManagment.DAL.Models;

namespace GymManagment.DAL.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Member> Members { get; }

        IGenericRepository<HealthRecord> HealthRecords { get; }
        IGenericRepository<Plans> Plans { get; }
        IGenericRepository<Session> Sessions { get; }
        IBookingRepository Bookings { get; }
        IGenericRepository<Category> Categories { get; }
        IGenericRepository<Membership> Memberships { get; }
        IGenericRepository<Trainer> Trainers { get; }
        IGenericRepository<DeleteRequest> DeleteRequests { get; }
        IGenericRepository<WeightProgressRecord> WeightProgressRecords { get; }
        public ISessionRepository SessionRepository { get; }
        IWorkoutLogRepository WorkoutLogs { get; }
        IBadgeRepository Badges { get; }
        IGenericRepository<MemberWorkoutPlan> MemberWorkoutPlans { get; }
        IGenericRepository<Exercise> Exercises { get; }

        Task<int> CompleteAsync(CancellationToken ct = default);
    }
}
