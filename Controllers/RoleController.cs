using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using GBEMiddlewareApi.Models;
using GBEMiddlewareApi.Data;
using Microsoft.AspNetCore.Authorization;

namespace GBEMiddlewareApi.Controllers
{
    /// <summary>
    /// The RoleController provides endpoints for managing roles and permissions.
    /// It contains endpoints to list roles, assign roles to users, create roles,
    /// as well as endpoints to create permissions, list all permissions, and assign or remove
    /// permissions from roles.
    /// </summary>
    [Authorize] // Requires authentication for all endpoints
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _dbContext; // Required for permission endpoints

        /// <summary>
        /// Initializes a new instance of the <see cref="RoleController"/> class.
        /// </summary>
        /// <param name="userManager">The UserManager for ApplicationUser.</param>
        /// <param name="roleManager">The RoleManager for IdentityRole.</param>
        /// <param name="dbContext">The application's DbContext for database access.</param>
        public RoleController(UserManager<ApplicationUser> userManager,
                              RoleManager<IdentityRole> roleManager,
                              ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
        }

        #region Role Endpoints

        /// <summary>
        /// GET: api/Role/all
        /// Retrieves a list of all role names.
        /// </summary>
        /// <returns>A list of role names.</returns>
        [HttpGet("all")]
        public IActionResult GetAllRoles()
        {
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return Ok(roles);
        }

        /// <summary>
        /// GET: api/Role/user-roles/{email}
        /// Retrieves the roles assigned to a user specified by email.
        /// </summary>
        /// <param name="email">The user's email address.</param>
        /// <returns>A list of roles assigned to the user.</returns>
        [HttpGet("user-roles/{email}")]
        public async Task<IActionResult> GetUserRoles(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound(new string[] { "User not found" });
            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }

        /// <summary>
        /// POST: api/Role/assign
        /// Assigns a role to a user.
        /// Expected payload: { "email": "user@example.com", "roleName": "Admin" }
        /// </summary>
        /// <param name="model">An object containing the user's email and role name.</param>
        /// <returns>Status message regarding the assignment.</returns>
        [HttpPost("assign")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new string[] { "Invalid model state" });

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return NotFound(new string[] { "User not found" });

            if (!await _roleManager.RoleExistsAsync(model.RoleName))
                return NotFound(new string[] { "Role does not exist" });

            var result = await _userManager.AddToRoleAsync(user, model.RoleName);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description).ToArray());

            return Ok(new string[] { "Role assigned successfully" });
        }

        /// <summary>
        /// POST: api/Role/remove
        /// Removes a role from a user.
        /// Expected payload: { "email": "user@example.com", "roleName": "Admin" }
        /// </summary>
        /// <param name="model">An object containing the user's email and role name.</param>
        /// <returns>Status message regarding the removal.</returns>
        [HttpPost("remove")]
        public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new string[] { "Invalid model state" });

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return NotFound(new string[] { "User not found" });

            if (!await _roleManager.RoleExistsAsync(model.RoleName))
                return NotFound(new string[] { "Role does not exist" });

            var result = await _userManager.RemoveFromRoleAsync(user, model.RoleName);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description).ToArray());

            return Ok(new string[] { "Role removed successfully" });
        }


        [HttpPost("create")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            var userPermissions = User.Claims.Where(c => c.Type == "Permission").Select(c => c.Value).ToList();

            if (!userPermissions.Contains("User_management"))
                return StatusCode(403, new { error = "Access denied. You do not have permission to create roles." });

            if (dto == null || string.IsNullOrWhiteSpace(dto.RoleName))
                return BadRequest(new string[] { "Role name is required." });

            var roleExists = await _roleManager.RoleExistsAsync(dto.RoleName);
            if (roleExists)
                return BadRequest(new string[] { "Role already exists." });

            var result = await _roleManager.CreateAsync(new IdentityRole(dto.RoleName));
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return BadRequest(errors);
            }

            return Ok(new string[] { "Role created successfully" });
        }

        #endregion

        #region Permission Endpoints

        /// <summary>
        /// POST: api/Role/permissions/create
        /// Creates a new permission.
        /// Expected payload: { "permissionName": "CanViewReports", "description": "Allows viewing reports" }
        /// </summary>
        /// <param name="model">An object containing the permission's name and description.</param>
        /// <returns>Status message regarding the creation.</returns>
        [HttpPost("permissions/create")]
        public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionDto model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.PermissionName))
            {
                return BadRequest(new string[] { "Permission name is required." });
            }

            // Check if the permission already exists.
            var existing = await _dbContext.Permissions
                .AnyAsync(p => p.Name == model.PermissionName);
            if (existing)
            {
                return BadRequest(new string[] { "Permission already exists." });
            }

            var permission = new Permission
            {
                Name = model.PermissionName,
                Description = model.Description
            };

            _dbContext.Permissions.Add(permission);
            await _dbContext.SaveChangesAsync();

            return Ok(new string[] { "Permission created successfully" });
        }

        /// <summary>
        /// GET: api/Role/permissions/all
        /// Retrieves all permissions with their details.
        /// </summary>
        /// <returns>A list of permissions including Id, Name, and Description.</returns>
        [HttpGet("permissions/all")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var permissions = await _dbContext.Permissions
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description
                })
                .ToListAsync();

            return Ok(permissions);
        }

        /// Retrieves all roles using roleID.


        [HttpGet("all-with-id")]
        public IActionResult GetAllRolesWithId()
        {
            // Return a list of { Id, Name } for each role
            var roles = _roleManager.Roles
                .Select(r => new { r.Id, r.Name })
                .ToList();
            return Ok(roles);
        }
        /// <summary>
        /// POST: api/Role/permissions/assign
        /// Assigns a permission to a role.
        /// Expected payload: { "roleName": "Admin", "permissionName": "CanViewReports" }
        /// </summary>
        /// <param name="model">An object containing the role name and permission name.</param>
        /// <returns>Status message regarding the assignment.</returns>
        [HttpPost("permissions/assign")]
        public async Task<IActionResult> AssignPermissionToRole([FromBody] AssignPermissionDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new string[] { "Invalid model state" });

            if (string.IsNullOrWhiteSpace(model.RoleName) ||
                string.IsNullOrWhiteSpace(model.PermissionName))
            {
                return BadRequest(new string[] { "Role name and permission name are required." });
            }

            // Retrieve the role by name.
            var role = await _roleManager.FindByNameAsync(model.RoleName);
            if (role == null)
                return NotFound(new string[] { "Role not found." });

            // Retrieve the permission by name.
            var permission = await _dbContext.Permissions
                .FirstOrDefaultAsync(p => p.Name == model.PermissionName);
            if (permission == null)
                return NotFound(new string[] { "Permission not found." });

            // Check if the role already has this permission.
            var alreadyAssigned = await _dbContext.RolePermissions
                .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);
            if (alreadyAssigned)
            {
                return BadRequest(new string[] { "Role already has this permission." });
            }

            var rolePermission = new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permission.Id
            };

            _dbContext.RolePermissions.Add(rolePermission);
            await _dbContext.SaveChangesAsync();

            return Ok(new string[] { "Permission assigned to role successfully" });
        }

        /// <summary>
        /// POST: api/Role/permissions/remove
        /// Removes a permission from a role.
        /// Expected payload: { "roleName": "Admin", "permissionName": "CanViewReports" }
        /// </summary>
        /// <param name="model">An object containing the role name and permission name.</param>
        /// <returns>Status message regarding the removal.</returns>
        [HttpPost("permissions/remove")]
        public async Task<IActionResult> RemovePermissionFromRole([FromBody] AssignPermissionDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new string[] { "Invalid model state" });

            // Retrieve the role by name.
            var role = await _roleManager.FindByNameAsync(model.RoleName);
            if (role == null)
                return NotFound(new string[] { "Role not found." });

            // Retrieve the permission by name.
            var permission = await _dbContext.Permissions
                .FirstOrDefaultAsync(p => p.Name == model.PermissionName);
            if (permission == null)
                return NotFound(new string[] { "Permission not found." });

            var rolePermission = await _dbContext.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);
            if (rolePermission == null)
            {
                return BadRequest(new string[] { "Role does not have this permission." });
            }

            _dbContext.RolePermissions.Remove(rolePermission);
            await _dbContext.SaveChangesAsync();

            return Ok(new string[] { "Permission removed from role successfully" });
        }


        /// <summary>
        /// GET: api/Role/permissions/role/{roleId}
        /// Retrieves all permissions (Id and Name) associated with the specified role
        /// from the RolePermissions table.
        /// </summary>
        /// <param name="roleId">The ID of the role.</param>
        /// <returns>A list of permission objects with Id and Name.</returns>
        [HttpGet("permissions/role/{roleId}")]
        public async Task<IActionResult> GetPermissionsByRole(string roleId)
        {
            // Retrieve the role using the roleId.
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return NotFound(new string[] { "Role not found" });

            // Retrieve associated permissions using a join on RolePermissions.
            var permissions = await _dbContext.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .Include(rp => rp.Permission)
                .Select(rp => new { rp.Permission.Id, rp.Permission.Name })
                .ToListAsync();

            return Ok(permissions);
        }

        /// <summary>
        /// DELETE: api/Role/permissions/delete/{permissionId}
        /// Deletes a permission by its ID.
        /// </summary>
        /// <param name="permissionId">The ID of the permission to delete.</param>
        /// <returns>Status message regarding the deletion.</returns>
        [HttpDelete("permissions/delete/{permissionId}")]
        public async Task<IActionResult> DeletePermission(int permissionId)
        {
            var permission = await _dbContext.Permissions.FindAsync(permissionId);
            if (permission == null)
                return NotFound(new string[] { "Permission not found." });

            // Check if this permission is assigned to any role before deleting
            var assignedRoles = await _dbContext.RolePermissions.AnyAsync(rp => rp.PermissionId == permissionId);
            if (assignedRoles)
                return BadRequest(new string[] { "Permission is assigned to a role and cannot be deleted." });

            _dbContext.Permissions.Remove(permission);
            await _dbContext.SaveChangesAsync();

            return Ok(new string[] { "Permission deleted successfully." });
        }

        // Get All roles With Permission
        [HttpGet("roles-with-permissions")]
        public async Task<IActionResult> GetRolesWithPermissions()
        {
            // Retrieve all roles from Identity
            var roles = await _roleManager.Roles.ToListAsync();

            // Retrieve all role-permission relationships including permission details
            var rolePermissions = await _dbContext.RolePermissions
                .Include(rp => rp.Permission)
                .ToListAsync();

            // Combine roles with their associated permissions
            var result = roles.Select(role => new
            {
                role.Id,
                role.Name,
                Permissions = rolePermissions
                    .Where(rp => rp.RoleId == role.Id)
                    .Select(rp => new
                    {
                        rp.Permission.Id,
                        rp.Permission.Name,
                        rp.Permission.Description
                    })
                    .ToList()
            });

            return Ok(result);
        }

        /// <summary>
        /// GET: api/Role/permissions/simple
        /// Retrieves all permissions with only their Id and Name.
        /// </summary>
        /// <returns>A list of all permissions with Id and Name.</returns>
        [HttpGet("permissions/simple")]
        public async Task<IActionResult> GetSimplePermissions()
        {
            var permissions = await _dbContext.Permissions
                .Select(p => new { p.Id, p.Name })
                .ToListAsync();
            return Ok(permissions);
        }

        #endregion
    }



    #region DTOs

    /// <summary>
    /// Data Transfer Object for assigning a role to a user.
    /// </summary>
    public class AssignRoleDto
    {
        /// <summary>
        /// The user's email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The name of the role to assign.
        /// </summary>
        public string RoleName { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for creating a new role.
    /// </summary>
    public class CreateRoleDto
    {
        /// <summary>
        /// The name of the new role.
        /// </summary>
        public string RoleName { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for creating a new permission.
    /// </summary>
    public class CreatePermissionDto
    {
        /// <summary>
        /// The name of the new permission.
        /// </summary>
        public string PermissionName { get; set; }

        /// <summary>
        /// A brief description of what the permission allows.
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for assigning a permission to a role.
    /// </summary>
    public class AssignPermissionDto
    {
        /// <summary>
        /// The name of the role.
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// The name of the permission to assign.
        /// </summary>
        public string PermissionName { get; set; }
    }

    #endregion
}
