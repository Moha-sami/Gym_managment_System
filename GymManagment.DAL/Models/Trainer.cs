using GymManagment.DAL.Models.Enum;

namespace GymManagment.DAL.Models
{
    public class Trainer:GymUser
    {
        public Specialty Specialty { get; set; }

        //Relations
        public ICollection<Session> Sessions { get; set; } = [];
    }
}
