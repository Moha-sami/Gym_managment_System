namespace GymMangment.BLL.ViewModels.BadgeViewModels
{
    public class BadgeDefinitionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string IconPath { get; set; } = default!;
        public string Category { get; set; } = default!;
        public string Tier { get; set; } = default!;
        public int? Threshold { get; set; }
        public bool IsAutomatic { get; set; }
    }

    public class MemberBadgeViewModel
    {
        public int Id { get; set; }
        public int BadgeDefinitionId { get; set; }
        public string BadgeName { get; set; } = default!;
        public string BadgeDescription { get; set; } = default!;
        public string BadgeIconPath { get; set; } = default!;
        public string BadgeTier { get; set; } = default!;
        public string BadgeCategory { get; set; } = default!;
        public DateTime EarnedAtUtc { get; set; }
        public string? AwardedByUserName { get; set; }
    }

    public class AchievementsPageViewModel
    {
        public string MemberName { get; set; } = default!;
        public int TotalBadgeCount { get; set; }
        public List<MemberBadgeViewModel> EarnedBadges { get; set; } = [];
        public List<BadgeDefinitionViewModel> AllDefinitions { get; set; } = [];
        public Dictionary<string, int> CategoryProgress { get; set; } = new();
    }

    public class LeaderboardEntryViewModel
    {
        public int Rank { get; set; }
        public int MemberId { get; set; }
        public string MemberName { get; set; } = default!;
        public string? MemberPhoto { get; set; }
        public int BadgeCount { get; set; }
        public string? LatestBadgeName { get; set; }
        public string? LatestBadgeIconPath { get; set; }
    }

    public class AwardBadgeViewModel
    {
        public int MemberId { get; set; }
        public string MemberName { get; set; } = default!;
        public int BadgeDefinitionId { get; set; }
        public List<BadgeDefinitionViewModel> AvailableBadges { get; set; } = [];
    }
}
