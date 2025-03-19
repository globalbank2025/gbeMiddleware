namespace GBEMiddlewareApi.Models
{
    public class ScheduledTask
    {
        public long TaskId { get; set; }
        public string TaskName { get; set; }
        public string CronExpression { get; set; }
        public DateTimeOffset? LastRunTime { get; set; }
        public DateTimeOffset? NextRunTime { get; set; }
        public string Status { get; set; } = "SCHEDULED";
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
