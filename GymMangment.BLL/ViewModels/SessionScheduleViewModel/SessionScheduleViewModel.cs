namespace GymMangment.BLL.ViewModels.BookingViewModels
{
    public class SessionScheduleViewModel
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = default!;
        public string TrainerName { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string DateDisplay { get; set; } = default!;
        public string TimeRangeDisplay { get; set; } = default!;
        public int AvailableSlots { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; } = default!;
        public bool CanBook => AvailableSlots > 0 && Status != "Completed";
    }
}