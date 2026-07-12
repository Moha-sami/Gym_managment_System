using System;

namespace GymManagment.DAL.Models
{
    public class MemberWorkoutPlan : BaseEntity
    {
        public int MemberId { get; set; }
        public Member Member { get; set; } = null!;
        public string Goal { get; set; } = string.Empty;
        public int Frequency { get; set; }
        public string ExperienceLevel { get; set; } = string.Empty;
        public string PlanJson { get; set; } = string.Empty;
    }
}
