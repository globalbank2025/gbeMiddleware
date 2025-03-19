namespace GBEMiddlewareApi.Models
{
    public class CreatePermissionDto
    {
        public string PermissionName { get; set; }
        public string Description { get; set; }
    }

    public class AssignPermissionDto
    {
        public string RoleName { get; set; }
        public string PermissionName { get; set; }
    }
}
