using Microsoft.AspNetCore.Authorization;

namespace GBEMiddlewareApi.Security
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string PermissionName { get; set; }

        public PermissionRequirement(string permissionName)
        {
            PermissionName = permissionName;
        }
    }
}
