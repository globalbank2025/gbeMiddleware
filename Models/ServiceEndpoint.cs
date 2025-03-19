namespace GBEMiddlewareApi.Models
{
    public class ServiceEndpoint
    {
        public long EndpointId { get; set; }
        public long ServiceId { get; set; }
        public string Environment { get; set; } // e.g., 'DEV', 'UAT', 'PROD'
        public string EndpointUrl { get; set; }
        public string HttpMethod { get; set; }
        public string SoapAction { get; set; }
        public int? ConnectionTimeout { get; set; }
        public int? ReadTimeout { get; set; }
        public string Status { get; set; } = "ACTIVE";
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation (optional)
        public Service Service { get; set; }
    }
}
