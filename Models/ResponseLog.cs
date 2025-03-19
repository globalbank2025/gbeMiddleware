namespace GBEMiddlewareApi.Models
{
    public class ResponseLog
    {
        public long ResponseId { get; set; }
        public long RequestId { get; set; }
        public DateTimeOffset ResponseTimestamp { get; set; } = DateTimeOffset.UtcNow;
        public int HttpStatusCode { get; set; }
        public string ResponseHeaders { get; set; }
        public string ResponsePayload { get; set; }
        public int? ProcessingTimeMs { get; set; }
        public string Status { get; set; } = "SUCCESS";
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation (optional)
        public RequestLog Request { get; set; }
    }
}
