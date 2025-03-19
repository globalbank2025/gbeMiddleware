using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GBEMiddlewareApi.Data;

namespace GBEMiddlewareApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public UserController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/User/list
        [HttpGet("list")]
        public async Task<IActionResult> GetUsersWithRoles()
        {
            // Include the Branch navigation property
            var users = await _dbContext.Users
                .Include(u => u.Branch)
                .ToListAsync();

            // Join with IdentityUserRole and Roles to obtain roles
            var userRoles = await (from ur in _dbContext.Set<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>()
                                   join r in _dbContext.Roles on ur.RoleId equals r.Id
                                   select new { ur.UserId, RoleName = r.Name })
                                   .ToListAsync();

            var result = users.Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                // Instead of BranchId, we return BranchName (or "N/A" if missing)
                BranchName = u.Branch != null ? u.Branch.BranchName : "N/A",
                Roles = userRoles
                         .Where(x => x.UserId == u.Id)
                         .Select(x => x.RoleName)
                         .ToList()
            }).ToList();

            return Ok(result);
        }
    }
}
