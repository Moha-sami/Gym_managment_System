namespace GymMangment.BLL.ViewModels.MemberViewModels
{
    public class MemberViewModel
    {
        public string? photo { get; set; }
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public string Gender { get; set; } = default!;

        //member Details
        public string? Address { get; set; } 

        public string? DateOfBirth { get; set; }

        public string? PlanName { get; set; }
        public string? MembershipStartDate { get; set; }
        public string? MembershipEndDate { get; set; }


    }
}
