using System.ComponentModel.DataAnnotations;

namespace GymMangment.BLL.ViewModels.AccountViewModels
{
    public class VerifyOtpViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "OTP is required.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be a numeric 6-digit code.")]
        public string Otp { get; set; } = string.Empty;
    }
}
