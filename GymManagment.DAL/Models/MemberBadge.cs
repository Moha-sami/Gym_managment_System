namespace GymManagment.DAL.Models
{
    public class MemberBadge : BaseEntity
    {
        public int MemberId { get; set; }
        public Member Member { get; set; } = default!;

        public int BadgeDefinitionId { get; set; }
        public BadgeDefinition BadgeDefinition { get; set; } = default!;

        public DateTime EarnedAtUtc { get; set; }

        // Nullable — null means auto-awarded, populated means admin manually awarded
        public string? AwardedByUserId { get; set; }
        public AppUser? AwardedByUser { get; set; }
    }
}
