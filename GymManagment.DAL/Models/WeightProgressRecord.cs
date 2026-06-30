namespace GymManagment.DAL.Models
{
    public class WeightProgressRecord : BaseEntity
    {
        public decimal Weight { get; set; }
        public DateTime RecordedAt { get; set; }
        public string? Note { get; set; }

        public int MemberId { get; set; }
        public Member Member { get; set; } = default!;
    }
}
