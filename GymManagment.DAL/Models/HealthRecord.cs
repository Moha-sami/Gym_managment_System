namespace GymManagment.DAL.Models
{
    public class HealthRecord:BaseEntity
    {
        public Decimal Height { get; set; }

        public Decimal Weight { get; set; }

        public String? Note { get; set; }
        public String? BloodType { get; set; }

        //LastUpdate we Get it From updatedAt from Baseentity

        //Relations
        public int MemberId { get; set; }
        public Member Member { get; set; }=default!;

    }
}
