using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using GBEMiddlewareApi.Data;
using Microsoft.EntityFrameworkCore;
using GBEMiddlewareApi.Models;

namespace GBEMiddlewareApi.Security
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public PermissionAuthorizationHandler(UserManager<ApplicationUser> userManager,
                                             ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                // User is not logged in => fail
                return;
            }

            // Get the user from database
            var user = await _userManager.GetUserAsync(context.User);
            if (user == null)
            {
                // Could not find user => fail
                return;
            }

            // Get all roles of this user
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles == null || !userRoles.Any())
            {
                return; // no roles => no permissions
            }

            // Check if any of the user's roles has the required permission
            var hasPermission = await _dbContext.RolePermissions
                .Include(rp => rp.Permission)
                .Include(rp => rp.Role)
                .AnyAsync(rp =>
                     userRoles.Contains(rp.Role.Name)
                     && rp.Permission.Name == requirement.PermissionName
                );

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
        }
    }
}
