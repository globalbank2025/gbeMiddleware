using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using GBEMiddlewareApi.Models;
using Microsoft.Extensions.Configuration;
using GBEMiddlewareApi.Data;
using Microsoft.AspNetCore.Authorization;

namespace GBEMiddlewareApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;

        public AuthController(UserManager<ApplicationUser> userManager,
                              IConfiguration configuration,
                              ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _configuration = configuration;
            _dbContext = dbContext;
        }

        // POST: api/auth/register
        [Authorize(Roles = "Admin")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                BranchId = model.BranchId
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }

            // Assign role if provided
            if (!string.IsNullOrEmpty(model.Role))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
                if (!roleResult.Succeeded)
                {
                    return BadRequest(new { Errors = roleResult.Errors.Select(e => e.Description) });
                }
            }

            return Ok("User registered successfully.");
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized(new { error = "Invalid credentials." });

            // Load related Branch details
            await _dbContext.Entry(user).Reference(u => u.Branch).LoadAsync();

            // Get user roles
            var userRoles = await _userManager.GetRolesAsync(user);

            // Fetch all distinct permissions for user's roles
            var userPermissions = await (
                from rp in _dbContext.RolePermissions
                join p in _dbContext.Permissions on rp.PermissionId equals p.Id
                join r in _dbContext.Roles on rp.RoleId equals r.Id
                where userRoles.Contains(r.Name)
                select p.Name
            ).Distinct().ToListAsync();

            // Build claims for JWT
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("BranchId", user.BranchId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add role claims
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Add permission claims
            foreach (var permission in userPermissions)
            {
                authClaims.Add(new Claim("Permission", permission));
            }

            // Generate JWT Token
            var jwtSecret = _configuration["JWT:Secret"];
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var expiryMinutes = Convert.ToDouble(_configuration["JWT:TokenExpiryInMinutes"]);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddMinutes(expiryMinutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            // Prepare user info response
            var userInfo = new
            {
                user.Id,
                user.UserName,
                user.Email,
                BranchId = user.BranchId,
                BranchName = user.Branch?.BranchName,
                BranchCode = user.Branch?.BranchCode,
                Roles = userRoles.ToList(),
                Permissions = userPermissions
            };

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                user = userInfo
            });
        }

        // POST: api/auth/logout
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok("Logout successful. Client should discard the token.");
        }

        // GET: api/auth/users (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _dbContext.Users.Include(u => u.Branch).ToListAsync();

            var userList = new List<UserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userList.Add(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    BranchId = user.BranchId,
                    BranchName = user.Branch?.BranchName,
                    Roles = roles.ToList()
                });
            }

            return Ok(userList);
        }
        
        // Password Reset Endpoint
        [Authorize(Roles = "Admin")]
        [HttpPost("reset-password/{userId}")]
        public async Task<IActionResult> ResetPassword(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { success = false, message = "User not found." });

            // Generate password reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var defaultPassword = "Gbe@1234";

            var result = await _userManager.ResetPasswordAsync(user, resetToken, defaultPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new { success = false, errors = result.Errors.Select(e => e.Description) });
            }

            return Ok(new { success = true, message = "Password has been reset to default." });
        }

        //  Lock user account (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPost("lock-user/{userId}")]
        public async Task<IActionResult> LockUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound(new { success = false, message = "User not found." });

            // Lock the account indefinitely
            var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            if (!result.Succeeded)
            {
                return BadRequest(new { success = false, errors = result.Errors.Select(e => e.Description) });
            }

            return Ok(new { success = true, message = "User account has been locked." });
        }
        // Password Reset By User It Self
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data." });

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                return BadRequest(new { success = false, message = "New password and confirmation do not match." });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new { success = false, message = "User not found." });

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new { success = false, errors = result.Errors.Select(e => e.Description) });
            }

            return Ok(new { success = true, message = "Password changed successfully." });
        }

    }


    // DTOs for request and response
    public class RegisterUserDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public int BranchId { get; set; }
        public string Role { get; set; }
    }

    public class LoginUserDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public List<string> Roles { get; set; }
    }
    // DTO for changing password
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }
}
