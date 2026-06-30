namespace GymMangment.BLL.ViewModels.MembershipViewModels
{
    public class MyMembershipViewModel
    {
        public string PlanName { get; set; } = default!;
        public string PlanDescription { get; set; } = default!;
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public int DaysRemaining { get; set; }
    }
}