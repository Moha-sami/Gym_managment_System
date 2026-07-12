using System;
using System.Collections.Generic;

namespace GymManagment.DAL.Models
{
    public class WorkoutLog : BaseEntity
    {
        public string Name { get; set; } = default!;
        public DateTime Date { get; set; }
        public string? Notes { get; set; }

        // Relations
        public int MemberId { get; set; }
        public Member Member { get; set; } = default!;

        public ICollection<WorkoutExerciseLog> Exercises { get; set; } = [];
    }
}
