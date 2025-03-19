using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GBEMiddlewareApi.Models
{
    public class RolePermission
    {
        // The role's primary key is a string in ASP.NET Identity
        [Required]
        public string RoleId { get; set; }

        [ForeignKey(nameof(RoleId))]
        public IdentityRole Role { get; set; }

        // The permission’s primary key is int
        [Required]
        public int PermissionId { get; set; }

        [ForeignKey(nameof(PermissionId))]
        public Permission Permission { get; set; }
    }
}
