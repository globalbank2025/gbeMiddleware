using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GBEMiddlewareApi.Models
{
    public enum CalculationType
    {
        Flat,
        Rate
    }

    public enum ServiceIncomeGlStatus
    {
        Open,
        Closed
    }

    public class ServiceIncomeGl : IValidatableObject
    {
        [Key]
        public int Id { get; set; }

        // Enforce exactly 7 digits (as a string to preserve leading zeros)
        [Required]
        [RegularExpression(@"^\d{7}$", ErrorMessage = "GL Code must be exactly 7 digits.")]
        public string GlCode { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        // Use JSON string conversion so that "Open"/"Closed" are parsed to enum values.
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ServiceIncomeGlStatus Status { get; set; }

        // Use JSON string conversion so that "Flat"/"Rate" are parsed to enum values.
        [Required]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CalculationType CalculationType { get; set; }

        // If CalculationType is Flat, a flat price must be provided.
        [Range(0, double.MaxValue, ErrorMessage = "Flat price must be a non-negative number.")]
        public decimal? FlatPrice { get; set; }

        // If CalculationType is Rate, a rate value must be provided.
        [Range(0, double.MaxValue, ErrorMessage = "Rate must be a non-negative number.")]
        public decimal? Rate { get; set; }

        // Conditional validation to ensure correct values are provided based on CalculationType.
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CalculationType == CalculationType.Flat && FlatPrice == null)
            {
                yield return new ValidationResult(
                    "FlatPrice is required when CalculationType is Flat.",
                    new[] { nameof(FlatPrice) });
            }
            if (CalculationType == CalculationType.Rate && Rate == null)
            {
                yield return new ValidationResult(
                    "Rate is required when CalculationType is Rate.",
                    new[] { nameof(Rate) });
            }
        }
    }
}
