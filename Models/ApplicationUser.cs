using Microsoft.AspNetCore.Identity;

namespace GBEMiddlewareApi.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Foreign key to Branch (must be supplied during registration)
        public int BranchId { get; set; }

        // Navigation property (optional for registration payload)
        public Branch Branch { get; set; }
    }
}
