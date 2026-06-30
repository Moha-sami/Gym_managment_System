using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GymMangment.BLL.ViewModels.BookingViewModels
{
    public class CreateBookingViewModel
    {
        [Required(ErrorMessage = "Session is required")]
        public int SessionId { get; set; }

        [Required(ErrorMessage = "Member is required")]
        public int MemberId { get; set; }

        public IEnumerable<SelectListItem> Members { get; set; } = [];
    }
}