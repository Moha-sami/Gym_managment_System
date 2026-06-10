using GymManagment.DAL.Models.Enum;

namespace GymManagment.DAL.Models
{
    public class Category:BaseEntity
    {
        public Categories CategoryName { get; set; }

        //Relations
        public ICollection<Session> Sessions { get; set; } = [];
    }
}
