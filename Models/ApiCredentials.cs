using System;

namespace GBEMiddlewareApi.Models
{
    public class ApiCredentials
    {
        public long ApiCredId { get; set; }
        public long PartnerId { get; set; }
        public long ServiceId { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; } // Store hashed or in secure vault
        public DateTimeOffset? TokenExpiry { get; set; }
        public string AllowedIp { get; set; }
        public string Status { get; set; } = "ACTIVE";
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Optional navigation
        public Partner Partner { get; set; }
        public Service Service { get; set; }
    }
}
