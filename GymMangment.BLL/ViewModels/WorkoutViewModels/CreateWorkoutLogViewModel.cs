using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GymMangment.BLL.ViewModels.WorkoutViewModels
{
    public class CreateWorkoutLogViewModel
    {
        [Required(ErrorMessage = "Workout name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Workout name must be between 2 and 100 characters")]
        public string Name { get; set; } = default!;

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; } = DateTime.Today;

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        public int MemberId { get; set; }

        public List<CreateWorkoutExerciseViewModel> Exercises { get; set; } = [];
    }
}
