using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using GBEMiddlewareApi.Data;
using Microsoft.EntityFrameworkCore;
using GBEMiddlewareApi.Models;
using System.Security.Claims;

namespace GBEMiddlewareApi.Security
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public PermissionAuthorizationHandler(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                return; // User is not authenticated
            }

            var user = await _userManager.GetUserAsync(context.User);
            if (user == null)
            {
                return; // No user found
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            if (!userRoles.Any())
            {
                return; // User has no roles assigned
            }

            var userPermissions = await (
                from rp in _dbContext.RolePermissions
                join p in _dbContext.Permissions on rp.PermissionId equals p.Id
                join r in _dbContext.Roles on rp.RoleId equals r.Id
                where userRoles.Contains(r.Name)
                select p.Name
            ).Distinct().ToListAsync();

            // Add permissions as claims
            var claimsIdentity = context.User.Identity as ClaimsIdentity;
            if (claimsIdentity != null)
            {
                foreach (var permission in userPermissions)
                {
                    claimsIdentity.AddClaim(new Claim("Permission", permission));
                }
            }

            if (userPermissions.Contains(requirement.PermissionName))
            {
                context.Succeed(requirement);
            }
        }
    }
}
