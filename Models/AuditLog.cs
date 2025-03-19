using System;
using System.ComponentModel.DataAnnotations;

namespace GBEMiddlewareApi.Models
{
    public class AuditLog
    {
        [Key]
        public long AuditId { get; set; }

        [Required, MaxLength(100)]
        public string TableName { get; set; }      // e.g., "VatCollectionTransactions"

        public long RecordId { get; set; }         // PK of the affected record

        [Required, MaxLength(20)]
        public string Operation { get; set; }      // e.g., "APPROVE", "REJECT", etc.

        public string OldData { get; set; }        // JSON string of old record state

        public string NewData { get; set; }        // JSON string of new record state

        [Required, MaxLength(50)]
        public string ChangedBy { get; set; }      // e.g., the user who made the change

        public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;

        // Optional: capture external SOAP request/response details
        public string ExternalRequest { get; set; }
        public string ExternalResponse { get; set; }
    }
}
