//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Design;

//namespace GymManagment.DAL.DbContext
//{
//    public class GymDbContextFactory : IDesignTimeDbContextFactory<GymDbcontext>
//    {
//        public GymDbcontext CreateDbContext(string[] args)
//        {
//            var optionsBuilder = new DbContextOptionsBuilder<GymDbcontext>();


//            var connectionString = "Server=db57580.public.databaseasp.net; Database=db57580; User Id=db57580; Password=4Dr=k-W7H2+t; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;";

//            optionsBuilder.UseSqlServer(connectionString);

//            return new GymDbcontext(optionsBuilder.Options);
//        }
//    }
//}