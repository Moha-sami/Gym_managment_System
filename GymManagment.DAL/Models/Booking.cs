namespace GymManagment.DAL.Models
{
    public class Booking: BaseEntity
    {
        public Member Member { get; set; } = default!;
        public int MemberId { get; set; }

        public Session Session { get; set; } = default!;
        public int SessionId { get; set; }

        //BookingDate => Created At from BaseEntity
        public bool IsAttended { get; set; }
    }
}
