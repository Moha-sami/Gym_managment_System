namespace GymManagment.DAL.Models
{
    public class Membership : BaseEntity
    {
        //StartDate we Get it From createdAt  from Baseentity

        public DateTime EndDate { get; set; }
        public bool IsActive => EndDate > DateTime.Now;
        public string Status => EndDate > DateTime.Now ? "Active" : "Expired";

        //Relations
        public Member Member { get; set; } = default!;
        public int MemberID { get; set; }

        public Plans Plans { get; set; }
        public int PlansID { get; set; }
    }
}
