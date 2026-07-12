using GymManagment.DAL.Models.Enum;

namespace GymManagment.DAL.Models
{
    public class BadgeDefinition : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string IconPath { get; set; } = default!;
        public BadgeCategory Category { get; set; }
        public BadgeTier Tier { get; set; }
        public int? Threshold { get; set; }
        public bool IsAutomatic { get; set; }

        // Relations
        public ICollection<MemberBadge> MemberBadges { get; set; } = [];
    }
}
