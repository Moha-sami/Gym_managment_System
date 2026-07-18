using GymManagment.DAL.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace GymManagment.DAL.DbContext
{
    public class GymDbcontext : IdentityDbContext<AppUser>
    {
        public GymDbcontext(DbContextOptions<GymDbcontext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // ← important for Identity tables
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            modelBuilder.Entity<HealthRecord>()
                .Property(h => h.Height)
                .HasPrecision(18, 2); // يعني 18 رقم، منهم 2 بعد العلامة

            modelBuilder.Entity<HealthRecord>()
                .Property(h => h.Weight)
                .HasPrecision(18, 2);

            modelBuilder.Entity<WeightProgressRecord>()
                .Property(w => w.Weight)
                .HasPrecision(18, 2);
        }
      

        public DbSet<Plans> Plans { get; set; }
        public DbSet<Member> Member { get; set; }
        public DbSet<Session> Session { get; set; }
        public DbSet<Booking> Booking { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<HealthRecord> HealthRecord { get; set; }
        public DbSet<Membership> Membership { get; set; }
        public DbSet<Trainer> Trainer { get; set; }
        public DbSet<DeleteRequest> DeleteRequests { get; set; }
        public DbSet<WeightProgressRecord> WeightProgressRecords { get; set; }
        public DbSet<WorkoutLog> WorkoutLogs { get; set; }
        public DbSet<WorkoutExerciseLog> WorkoutExerciseLogs { get; set; }
        public DbSet<WorkoutSetLog> WorkoutSetLogs { get; set; }
        public DbSet<BadgeDefinition> BadgeDefinitions { get; set; }
        public DbSet<MemberBadge> MemberBadges { get; set; }
        public DbSet<MemberWorkoutPlan> MemberWorkoutPlans { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
    }
}
