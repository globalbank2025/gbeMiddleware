namespace GBEMiddlewareApi.Models
{
    public class Transaction
    {
        public long TransactionId { get; set; }
        public long RequestId { get; set; }
        public long PartnerId { get; set; }
        public long ServiceId { get; set; }
        public string TransactionRef { get; set; }
        public string TransactionType { get; set; }
        public decimal? Amount { get; set; }
        public string Currency { get; set; }
        public string TransactionStatus { get; set; } = "PENDING";
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation (optional)
        public RequestLog Request { get; set; }
        public Partner Partner { get; set; }
        public Service Service { get; set; }
    }
}
