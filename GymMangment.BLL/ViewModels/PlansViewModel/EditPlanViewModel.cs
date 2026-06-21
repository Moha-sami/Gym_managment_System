using System.ComponentModel.DataAnnotations;

namespace GymMangment.BLL.ViewModels.PlansViewModels
{
    public class EditPlanViewModel
    {
        public int Id { get; set; }

        public string? Name { get; set; } // display only, locked

        [Required(ErrorMessage = "Description Is Required")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Description must be between 5 and 500 characters")]
        public string Description { get; set; } = default!;

        [Required(ErrorMessage = "Price Is Required")]
        [Range(0.01, 100000, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Range(1, 3650, ErrorMessage = "Duration must be between 1 and 3650 days")]
        public int DurationInDays { get; set; }
    }
}