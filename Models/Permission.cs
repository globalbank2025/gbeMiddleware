using System.ComponentModel.DataAnnotations;

namespace GBEMiddlewareApi.Models
{
    public class Permission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        // Optional description
        public string Description { get; set; }
    }
}
