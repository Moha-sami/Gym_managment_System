using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace GymMangment.BLL.ViewModels.MembershipViewModels
{
    public class CreateMembershipViewModel
    {
        [Required(ErrorMessage = "Member is required")]
        public int MemberID { get; set; }

        [Required(ErrorMessage = "Plan is required")]
        public int PlansID { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        // Dropdowns
        public IEnumerable<SelectListItem> Members { get; set; } = [];
        public IEnumerable<SelectListItem> Plans { get; set; } = [];
    }
}