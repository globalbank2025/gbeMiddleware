namespace GBEMiddlewareApi.Models
{
    public class MappingRule
    {
        public long MappingId { get; set; }
        public long ServiceId { get; set; }
        public string SourceFormat { get; set; }   // e.g., 'JSON', 'XML'
        public string TargetFormat { get; set; }   // e.g., 'SOAP_XML', 'FLEXCUBE_XML'
        public string MappingRules { get; set; }   // DSL, XSLT, or JSON-based rules
        public string Direction { get; set; } = "INBOUND";
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation (optional)
        public Service Service { get; set; }
    }
}
