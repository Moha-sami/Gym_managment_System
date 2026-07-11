using GymManagment.DAL.Models.Enum;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GymMangment.BLL.ViewModels.MemberViewModels;

public class MemberToUpdateViewModel
{
    public int Id { get; set; }
    public string? Name { get; set; }       // display only, locked
    public DateOnly DateOfBirth { get; set; } // display only, locked
    public string? Gender { get; set; }      // display only, locked
    public string? CurrentPhoto { get; set; } // existing photo path for display

    [DataType(DataType.Upload)]
    public IFormFile? Photo { get; set; }   // new photo upload (optional)
    public string? PhotoPath { get; set; }  // saved path after upload

    [Required(ErrorMessage = "Email Is Required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "Phone Number Is Required")]
    [RegularExpression(@"^(010|011|012|015)\d{8}$", ErrorMessage = "Phone number must be a valid Egyptian mobile number")]
    public string Phone { get; set; } = default!;

    [Required(ErrorMessage = "Building Number Is Required")]
    [Range(1, int.MaxValue, ErrorMessage = "Building Number must be greater than 0")]
    public int BuildingNumber { get; set; }

    [Required(ErrorMessage = "City Is Required")]
    [StringLength(30, MinimumLength = 2)]
    [RegularExpression(@"^[a-zA-Z\s]+$")]
    public string City { get; set; } = default!;

    [Required(ErrorMessage = "Street Is Required")]
    [StringLength(30, MinimumLength = 2)]
    [RegularExpression(@"^[a-zA-Z0-9\s]+$")]
    public string Street { get; set; } = default!;

    // Health Record
    [Range(0.1, 300, ErrorMessage = "Height must be greater than 0")]
    public decimal Height { get; set; }

    [Range(0.1, 500, ErrorMessage = "Weight must be greater than 0")]
    public decimal Weight { get; set; }

    [StringLength(3, ErrorMessage = "Blood type must be 3 characters or less")]
    public string? BloodType { get; set; }

    public string? Note { get; set; }

    public decimal? BMI
    {
        get
        {
            if (Height > 0 && Weight > 0)
            {
                var heightInMeters = Height / 100;
                return Math.Round(Weight / (heightInMeters * heightInMeters), 1);
            }
            return null;
        }
    }

    public string BMICategory
    {
        get
        {
            var bmi = BMI;
            if (!bmi.HasValue) return "N/A";
            if (bmi < 18.5m) return "Underweight";
            if (bmi < 25.0m) return "Normal weight";
            if (bmi < 30.0m) return "Overweight";
            return "Obese";
        }
    }
}