using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity;

namespace GymManagment.DAL.Models
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = default!;
        public int? MemberId { get; set; }
        public Member? Member { get; set; }
        public int? TrainerId { get; set; }
        public Trainer? Trainer { get; set; }
    }
}