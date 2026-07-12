using System;
using System.Collections.Generic;

namespace GymMangment.BLL.ViewModels.AnalyticsViewModels
{
    public class WeightRecordPoint
    {
        public string DateLabel { get; set; } = default!;
        public decimal Weight { get; set; }
    }

    public class FrequencyPoint
    {
        public string PeriodLabel { get; set; } = default!; // e.g. "Week of 2026-06-01" or "June 2026"
        public int WorkoutCount { get; set; }
    }

    public class VolumePoint
    {
        public string DateLabel { get; set; } = default!;
        public int Volume { get; set; }
    }

    public class ExerciseProgressPoint
    {
        public string DateLabel { get; set; } = default!;
        public double MaxWeight { get; set; }
        public double Estimated1RM { get; set; }
    }

    public class MemberAnalyticsViewModel
    {
        public int MemberId { get; set; }
        public string MemberName { get; set; } = default!;
        public List<WeightRecordPoint> WeightRecords { get; set; } = [];
        public List<FrequencyPoint> WorkoutFrequency { get; set; } = [];
        public List<VolumePoint> WorkoutVolume { get; set; } = [];
        public List<string> LoggedExercises { get; set; } = [];
    }
}
