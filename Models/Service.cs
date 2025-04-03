using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GBEMiddlewareApi.Models
{
    public class Service
    {
        public long ServiceId { get; set; }

        public string ServiceCode { get; set; }

        public string ServiceName { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Indicates the type of service, e.g., "Credit" or "Debit".
        /// </summary>
        public string ServiceType { get; set; }

        /// <summary>
        /// The offset account number for third-party transactions.
        /// </summary>
        public string OffsetAccNo { get; set; }

        public string Status { get; set; } = "ACTIVE";

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Non-nullable product code with a default value.
        /// </summary>
        [Required(ErrorMessage = "Product Code is required.")]
        [DefaultValue("FTRQ")]
        public string ProductCode { get; set; } = "FTRQ";
        
        [Required(ErrorMessage = "Source Code is required.")]
        [DefaultValue("TLB")]
        public string SourceCode { get; set; } = "TLB";
    }
}
