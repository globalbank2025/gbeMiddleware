namespace GBEMiddlewareApi.Models
{
    public class ErrorLog
    {
        public long ErrorId { get; set; }
        public long? TransactionId { get; set; }
        public long? RequestId { get; set; }
        public DateTimeOffset ErrorTimestamp { get; set; } = DateTimeOffset.UtcNow;
        public string ErrorSeverity { get; set; } = "HIGH";
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
        public string Status { get; set; } = "OPEN";
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation (optional)
        public Transaction Transaction { get; set; }
        public RequestLog Request { get; set; }
    }
}
