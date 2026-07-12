using GymManagment.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymManagment.DAL.ModelsConfigrations
{
    internal class MemberBadgeConfig : IEntityTypeConfiguration<MemberBadge>
    {
        public void Configure(EntityTypeBuilder<MemberBadge> builder)
        {
            builder.HasKey(mb => mb.Id);

            // Prevent duplicate awards — a member can only earn each badge once
            builder.HasIndex(mb => new { mb.MemberId, mb.BadgeDefinitionId })
                   .IsUnique();

            // Member relationship — cascade delete (if member is deleted, remove their badges)
            builder.HasOne(mb => mb.Member)
                   .WithMany(m => m.MemberBadges)
                   .HasForeignKey(mb => mb.MemberId)
                   .OnDelete(DeleteBehavior.Cascade);

            // BadgeDefinition relationship — restrict (can't delete a badge definition if awarded)
            builder.HasOne(mb => mb.BadgeDefinition)
                   .WithMany(bd => bd.MemberBadges)
                   .HasForeignKey(mb => mb.BadgeDefinitionId)
                   .OnDelete(DeleteBehavior.Restrict);

            // AppUser relationship — set null on delete (if admin user is deleted, badge stays)
            builder.HasOne(mb => mb.AwardedByUser)
                   .WithMany()
                   .HasForeignKey(mb => mb.AwardedByUserId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
