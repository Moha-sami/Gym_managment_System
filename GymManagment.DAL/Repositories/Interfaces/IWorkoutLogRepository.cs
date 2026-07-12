using GymManagment.DAL.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GymManagment.DAL.Repositories.Interfaces
{
    public interface IWorkoutLogRepository : IGenericRepository<WorkoutLog>
    {
        Task<IEnumerable<WorkoutLog>> GetWorkoutsWithExercisesAndSetsAsync(int memberId, CancellationToken ct = default);
        Task<WorkoutLog?> GetWorkoutWithExercisesAndSetsAsync(int id, int memberId, CancellationToken ct = default);
    }
}
