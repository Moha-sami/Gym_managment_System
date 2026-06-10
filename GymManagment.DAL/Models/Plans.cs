namespace GymManagment.DAL.Models
{
    public class Plans: BaseEntity
    {
        
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
        public bool IsActive { get; set; }

        //Relations
        public ICollection<Membership> Memberships { get; set; } = [];

    }
}
