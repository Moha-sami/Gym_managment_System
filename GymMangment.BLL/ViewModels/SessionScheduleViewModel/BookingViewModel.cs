namespace GymMangment.BLL.ViewModels.BookingViewModels
{
    public class BookingViewModel
    {
        public int Id { get; set; }
        public string MemberName { get; set; } = default!;
        public string SessionCategory { get; set; } = default!;
        public string SessionDate { get; set; } = default!;
        public string SessionTime { get; set; } = default!;
        public bool IsAttended { get; set; }
        public int MemberId { get; set; }
        public int SessionId { get; set; }
        public DateTime BookingDate { get; set; }
    }
}