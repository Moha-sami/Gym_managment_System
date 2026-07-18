using System;

namespace GymManagment.DAL.Models
{
    public class Exercise : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string MuscleGroup { get; set; } = default!;
        public string VideoUrl { get; set; } = default!;
        public string Difficulty { get; set; } = default!;
    }
}
