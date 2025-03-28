using System;
using System.Text.Json.Serialization;

namespace GBEMiddlewareApi.Models
{
    public class ApiCredentials
    {
        public long ApiCredId { get; set; }
        public long PartnerId { get; set; }
        public long ServiceId { get; set; }

        // For token-based approach
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }

        // For username/password approach
        public string Username { get; set; }
        public string Password { get; set; } // In production, store a hashed value

        public DateTimeOffset? TokenExpiry { get; set; }
        public string AllowedIp { get; set; }
        public string Status { get; set; } = "ACTIVE";
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation properties
        // Mark them as nullable and ignore them in JSON binding so that the incoming JSON doesn't require them.
        [JsonIgnore]
        public Partner? Partner { get; set; }

        [JsonIgnore]
        public Service? Service { get; set; }
    }
}
