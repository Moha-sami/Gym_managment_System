using System.ComponentModel.DataAnnotations;

namespace GymMangment.BLL.ViewModels
{
    public class AnnouncementViewModel
    {
        [Required(ErrorMessage = "Announcement message is required")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Announcement message must be between 5 and 500 characters")]
        public string Message { get; set; } = default!;

        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Style class is required")]
        public string StyleClass { get; set; } = "info"; // Mapped to Bootstrap alert classes (info, warning, danger, success)
    }
}
