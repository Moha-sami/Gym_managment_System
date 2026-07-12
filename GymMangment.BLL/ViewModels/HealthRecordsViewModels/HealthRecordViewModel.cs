using System.ComponentModel.DataAnnotations;

namespace GymMangment.BLL.ViewModels.HealthRecordsViewModels
{
    public class HealthRecordViewModel
    {
        public int MemberId { get; set; }

        [Range(0.1, 300, ErrorMessage = "Height must be greater than 0")]
        public decimal Height { get; set; }

        [Range(0.1, 500, ErrorMessage = "Weight must be greater than 0")]
        public decimal Weight { get; set; }

        [Required(ErrorMessage = "Blood Type Is Required")]
        [StringLength(3, ErrorMessage = "Blood type must be 3 characters or less")]
        public string BloodType { get; set; } = default!;
        public string? Note { get; set; } = default!;

        public IEnumerable<WeightProgressPointViewModel> WeightProgress { get; set; } = [];

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
}
