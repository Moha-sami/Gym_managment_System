using GymManagment.DAL.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace GymManagment.DAL.Models
{
    public abstract class GymUser:BaseEntity
    {
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public DateOnly DateOFBirth { get; set; }

        public Gender Gender { get; set; }
        public Address Address { get; set; }


        //Gender
        //Address
        // Building No  ○ Street  ○ City



    }
    [Owned]
    public class Address
    {
        public string City { get; set; }
        public string Street { get; set; }
        public int BuildingNumber { get; set; }
    }
}
