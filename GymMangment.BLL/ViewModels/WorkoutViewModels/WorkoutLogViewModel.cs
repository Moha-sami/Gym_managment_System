using System;
using System.Collections.Generic;
using System.Linq;

namespace GymMangment.BLL.ViewModels.WorkoutViewModels
{
    public class WorkoutLogViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public DateTime Date { get; set; }
        public string? Notes { get; set; }
        public int MemberId { get; set; }
        public ICollection<WorkoutExerciseViewModel> Exercises { get; set; } = [];

        public decimal TotalVolume => Exercises.Sum(e => e.Volume);
    }
}
