namespace GymMangment.BLL.ViewModels.MemberViewModels
{
    public class MemberViewModel
    {
        public string? photo { get; set; }
        public int Id { get; set; }
        public string name { get; set; }=default!;
        public string Email { get; set; }= default!;
        public string phone { get; set; } = default!;
        public string gender { get; set; } = default!;

    }
}
