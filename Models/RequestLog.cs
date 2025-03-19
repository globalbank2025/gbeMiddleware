namespace GBEMiddlewareApi.Models
{
    public class RequestLog
    {
        public long RequestId { get; set; }
        public long PartnerId { get; set; }
        public long ServiceId { get; set; }
        public DateTimeOffset RequestTimestamp { get; set; } = DateTimeOffset.UtcNow;
        public string HttpMethod { get; set; }
        public string RequestHeaders { get; set; }  // Could map JSONB to a string or a custom type
        public string RequestPayload { get; set; }  // Same comment as above
        public string ClientIp { get; set; }
        public string CorrelationId { get; set; }
        public string Status { get; set; } = "RECEIVED";
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation (optional)
        public Partner Partner { get; set; }
        public Service Service { get; set; }
    }
}
