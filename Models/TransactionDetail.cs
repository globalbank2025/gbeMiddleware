namespace GBEMiddlewareApi.Models
{
    public class TransactionDetail
    {
        public long TxDetailId { get; set; }
        public long TransactionId { get; set; }
        public int StepSequence { get; set; }
        public string StepName { get; set; }
        public string RequestPayload { get; set; }
        public string ResponsePayload { get; set; }
        public string Status { get; set; } = "SUCCESS";
        public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? EndedAt { get; set; }
        public string ErrorMessage { get; set; }

        // Navigation (optional)
        public Transaction Transaction { get; set; }
    }
}
