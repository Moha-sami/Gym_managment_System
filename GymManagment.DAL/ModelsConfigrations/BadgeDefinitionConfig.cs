using GymManagment.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagment.DAL.ModelsConfigrations
{
    internal class BadgeDefinitionConfig : IEntityTypeConfiguration<BadgeDefinition>
    {
        public void Configure(EntityTypeBuilder<BadgeDefinition> builder)
        {
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Name).IsRequired().HasMaxLength(100);
            builder.Property(b => b.Description).IsRequired().HasMaxLength(300);
            builder.Property(b => b.IconPath).IsRequired().HasMaxLength(200);

            // Store enums as strings for readability
            builder.Property(b => b.Category)
                   .HasConversion<string>()
                   .HasMaxLength(50);

            builder.Property(b => b.Tier)
                   .HasConversion<string>()
                   .HasMaxLength(10);
        }
    }
}
