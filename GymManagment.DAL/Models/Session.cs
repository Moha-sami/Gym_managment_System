namespace GymManagment.DAL.Models
{
    public class Session:BaseEntity
    {
        public string Description { get; set; }

        public int Capacity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        //Relations
        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; } = default!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = default!;

        public ICollection<Booking> SessionMembers { get; set; } = [];
    }
}
