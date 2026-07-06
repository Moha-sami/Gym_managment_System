using GymManagment.DAL.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace GymMangment.BLL.ViewModels.AccountViewModels;

public class RegisterExternalViewModel
{
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Provider { get; set; } = default!; // "Google" or "Facebook"

    [Required(ErrorMessage = "Phone Number Is Required")]
    [RegularExpression(@"^(010|011|012|015)\d{8}$", ErrorMessage = "Must be a valid Egyptian mobile number")]
    public string Phone { get; set; } = default!;

    [Required(ErrorMessage = "Date of Birth is required")]
    [DataType(DataType.Date)]
    public DateOnly DateOfBirth { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    public Gender Gender { get; set; }

    [Required(ErrorMessage = "Building Number Is Required")]
    [Range(1, 9000)]
    public int BuildingNumber { get; set; }

    [Required(ErrorMessage = "City Is Required")]
    [StringLength(30, MinimumLength = 2)]
    [RegularExpression(@"^[a-zA-Z\s]+$")]
    public string City { get; set; } = default!;

    [Required(ErrorMessage = "Street Is Required")]
    [StringLength(30, MinimumLength = 2)]
    [RegularExpression(@"^[a-zA-Z0-9\s]+$")]
    public string Street { get; set; } = default!;
}