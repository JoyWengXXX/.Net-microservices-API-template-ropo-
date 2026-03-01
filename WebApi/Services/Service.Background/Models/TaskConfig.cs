
namespace Service.Background.Models
{
    public class TaskConfig
    {
        public string TaskName { get; set; }
        public string TimeOfDay { get; set; }  // 用於固定時間執行的任務
        public TimeIntervalConfig Interval { get; set; }  // 用於間隔執行的任務
    }

    public class TimeIntervalConfig
    {
        public int? Days { get; set; }
        public int? Hours { get; set; }
        public int? Minutes { get; set; }
        public int? Seconds { get; set; }

        public TimeSpan? ToTimeSpan()
        {
            if (!Days.HasValue && !Hours.HasValue && !Minutes.HasValue && !Seconds.HasValue)
            {
                return null;
            }

            return new TimeSpan(
                Days ?? 0,
                Hours ?? 0,
                Minutes ?? 0,
                Seconds ?? 0);
        }
    }

    public class ScheduledTask
    {
        public TaskConfig Config { get; set; }
        public DateTime NextRun { get; set; }
    }
}

