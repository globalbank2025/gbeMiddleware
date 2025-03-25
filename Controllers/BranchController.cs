using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using GBEMiddlewareApi.Data;
using GBEMiddlewareApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Security.Claims;

namespace GBEMiddlewareApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requires authentication for all endpoints
    public class BranchController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BranchController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/branch
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Branch>>> GetAllBranches()
        {
            return await _context.Branches.ToListAsync();
        }

        // GET: api/branch/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Branch>> GetBranch(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
                return NotFound();
            return branch;
        }

        // POST: api/branch
        [HttpPost]
        public async Task<ActionResult<Branch>> CreateBranch(Branch branch)
        {
            // 🔹 Extract permissions from JWT claims
            var userPermissions = User.Claims
                .Where(c => c.Type == "Permission")
                .Select(c => c.Value)
                .ToList();

            // 🔹 Check if the user has "VatCollection_Create" permission
            if (!userPermissions.Contains("CanManageSystemSettings"))
            {
                return StatusCode(403, new { error = "Access denied. You do not have permission to create branch." });
            }

            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBranch), new { id = branch.BranchId }, branch);
        }


        // PUT: api/branch/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBranch(int id, Branch branchData)
        {
            if (id != branchData.BranchId)
                return BadRequest("ID mismatch.");

            var existing = await _context.Branches.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.BranchCode = branchData.BranchCode;
            existing.BranchName = branchData.BranchName;
            existing.Location = branchData.Location;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/branch/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBranch(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
                return NotFound();

            _context.Branches.Remove(branch);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
