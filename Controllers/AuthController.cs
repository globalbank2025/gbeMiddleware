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
        // Only an Admin can register new users.
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
                BranchId = model.BranchId  // Branch selected during registration
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Errors = errors });
            }

            // If a role is provided, assign it to the user.
            if (!string.IsNullOrEmpty(model.Role))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
                if (!roleResult.Succeeded)
                {
                    var errors = roleResult.Errors.Select(e => e.Description);
                    return BadRequest(new { Errors = errors });
                }
            }

            return Ok("User registered successfully.");
        }

        // POST: api/auth/login
        //[HttpPost("login")]
        //public async Task<IActionResult> Login([FromBody] LoginUserDto model)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    // Find the user by email.
        //    var user = await _userManager.FindByEmailAsync(model.Email);
        //    if (user == null)
        //        return Unauthorized("Invalid credentials.");

        //    // Verify the password.
        //    if (!await _userManager.CheckPasswordAsync(user, model.Password))
        //        return Unauthorized("Invalid credentials.");

        //    // Load related Branch information (including BranchCode)
        //    await _dbContext.Entry(user).Reference(u => u.Branch).LoadAsync();

        //    // Retrieve the user's roles.
        //    var userRoles = await _userManager.GetRolesAsync(user);

        //    // Create claims.
        //    var authClaims = new List<Claim>
        //    {
        //        new Claim(ClaimTypes.Name, user.UserName),
        //        new Claim("BranchId", user.BranchId.ToString()),
        //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        //    };

        //    // Add role claims.
        //    foreach (var userRole in userRoles)
        //    {
        //        authClaims.Add(new Claim(ClaimTypes.Role, userRole));
        //    }

        //    var jwtSecret = _configuration["JWT:Secret"];
        //    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

        //    // Use configuration value for token expiry in minutes
        //    var expiryMinutes = Convert.ToDouble(_configuration["JWT:TokenExpiryInMinutes"]);
        //    var token = new JwtSecurityToken(
        //        issuer: _configuration["JWT:ValidIssuer"],
        //        audience: _configuration["JWT:ValidAudience"],
        //        expires: DateTime.Now.AddMinutes(expiryMinutes),
        //        claims: authClaims,
        //        signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        //    );

        //    // Build a user info object to return including BranchCode.
        //    var userInfo = new
        //    {
        //        user.Id,
        //        user.UserName,
        //        user.Email,
        //        BranchId = user.BranchId,
        //        BranchName = user.Branch?.BranchName,
        //        BranchCode = user.Branch?.BranchCode,
        //        Roles = userRoles.ToList()
        //    };

        //    return Ok(new
        //    {
        //        token = new JwtSecurityTokenHandler().WriteToken(token),
        //        expiration = token.ValidTo,
        //        user = userInfo
        //    });
        //}


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1) Find the user by email.
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized("Invalid credentials.");

            // 2) Verify the password.
            if (!await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized("Invalid credentials.");

            // 3) Load related Branch information (including BranchCode)
            await _dbContext.Entry(user).Reference(u => u.Branch).LoadAsync();

            // 4) Retrieve the user's roles.
            var userRoles = await _userManager.GetRolesAsync(user); // role names, e.g. ["Admin", "Manager"]

            // 4a) Retrieve all distinct permissions for these roles. 
            //     We'll do a join between RolePermissions, Permissions, and Roles,
            //     filtering by role name. Then we select the Permission.Name.
            var userPermissionNames = await (
                from rp in _dbContext.RolePermissions
                join p in _dbContext.Permissions on rp.PermissionId equals p.Id
                join r in _dbContext.Roles on rp.RoleId equals r.Id
                where userRoles.Contains(r.Name)
                select p.Name
            )
            .Distinct()
            .ToListAsync();

            // 5) Build claims for JWT.
            var authClaims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim("BranchId", user.BranchId.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            // Add role claims
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            // 6) Build the token
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

            // 7) Build a user info object to return, including permissions
            var userInfo = new
            {
                user.Id,
                user.UserName,
                user.Email,
                BranchId = user.BranchId,
                BranchName = user.Branch?.BranchName,
                BranchCode = user.Branch?.BranchCode,
                Roles = userRoles.ToList(),
                Permissions = userPermissionNames // <--- Add the list of permission names
            };

            // 8) Return token + user info
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                user = userInfo
            });
        }



        // POST: api/auth/logout
        // In token-based authentication, logout is typically handled client-side.
        // This endpoint is provided as a stub.
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // In a real implementation, you might invalidate the token using a token revocation strategy.
            return Ok("Logout successful (client should discard the token).");
        }

        // GET: api/auth/users
        // Only an Admin can retrieve the list of users.
        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            // Retrieve all users including their Branch data.
            var users = await _dbContext.Users
                .Include(u => u.Branch)
                .ToListAsync();

            // Prepare a list of user DTOs.
            var userList = new List<UserDto>();

            foreach (var user in users)
            {
                // Retrieve roles for the current user.
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
    }

    // DTO for registering a user.
    public class RegisterUserDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public int BranchId { get; set; }
        public string Role { get; set; }
    }

    // DTO for logging in a user.
    public class LoginUserDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    // DTO for returning user information in the listing endpoint.
    public class UserDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public List<string> Roles { get; set; }
    }
}
