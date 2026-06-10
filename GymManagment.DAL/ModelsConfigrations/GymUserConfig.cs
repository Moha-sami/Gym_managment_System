using GymManagment.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagment.DAL.ModelsConfigrations
{
    public class GymUserConfig<T> : IEntityTypeConfiguration<T> where T : GymUser
    {
        public void Configure(EntityTypeBuilder<T> builder)
        {
            builder.Property(p => p.Name)
                 .HasColumnType("varchar").HasMaxLength(50);
            builder.HasIndex(p => p.Email).IsUnique();
            builder.HasIndex(p => p.Phone).IsUnique();
            builder.Property(p => p.Email)
                .HasColumnType("varchar").HasMaxLength(100);

           builder.ToTable(tb=> {
               tb.HasCheckConstraint("CK_GymUser_Phone", "Phone like '01%' and len(Phone) = 11");
               tb.HasCheckConstraint("CK_GymUser_Email", "Email like '%@%' and Email like '%.%'");
           });

           builder.OwnsOne(p => p.Address, Address=>{
               Address.Property(p => p.Street).HasColumnName("Street").HasColumnType("varchar").HasMaxLength(30);
               Address.Property(p => p.City).HasColumnName("City").HasColumnType("varchar").HasMaxLength(30);
               Address.Property(p => p.BuildingNumber).HasColumnName("BuildingNumber");
           });


        }
    }
}
