using GymManagment.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace GymManagment.DAL.DbContext
{
    public class GymDbcontext: Microsoft.EntityFrameworkCore.DbContext
    {
        public GymDbcontext(DbContextOptions<GymDbcontext> options): base(options) 
        {
            
        }

      
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        }
        public DbSet<Plans> Plans { get; set; }
        public DbSet<Member> Member { get; set; }
        public DbSet<Session> Session { get; set; }
        public DbSet<Booking> Booking { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<HealthRecord> HealthRecord { get; set; }
        public DbSet<Membership> Membership { get; set; }
        public DbSet<Trainer> Trainer { get; set; }


        
    }
}
