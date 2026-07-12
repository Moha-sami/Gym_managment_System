namespace GymManagment.DAL.Models
{
    public class Member:GymUser
    {
        public String? Photo { get; set; }

        //Join Date we get it From CreatedAt from Base Class and sql Getdate
        #region Relations
        public HealthRecord HealthRecord { get; set; } = default!;
         public ICollection<Membership> Memberships { get; set; } = [];

        public ICollection<Booking> MemberSessions { get; set; } = [];
        public ICollection<WeightProgressRecord> WeightProgressRecords { get; set; } = [];
        public ICollection<WorkoutLog> WorkoutLogs { get; set; } = [];
        public ICollection<MemberBadge> MemberBadges { get; set; } = [];
        #endregion
    }
}
