using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GBEMiddlewareApi.Models
{
    public class Branch
    {
        public int BranchId { get; set; }  // Auto-increment primary key
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string Location { get; set; }

        // Initialize to an empty list to prevent null errors.
        // Also, use [ValidateNever] so that MVC does not validate this navigation property.
        [JsonIgnore]
        [ValidateNever]
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}
