using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using GBEMiddlewareApi.Data;
using GBEMiddlewareApi.Models;

namespace GBEMiddlewareApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiCredentialsController : ControllerBase
    {
        private readonly MiddlewareDbContext _context;

        public ApiCredentialsController(MiddlewareDbContext context)
        {
            _context = context;
        }

        // GET: api/apicredentials
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApiCredentials>>> GetAll()
        {
            return await _context.ApiCredentials.ToListAsync();
        }

        // GET: api/apicredentials/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiCredentials>> Get(long id)
        {
            var item = await _context.ApiCredentials.FindAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        // POST: api/apicredentials
        [HttpPost]
        public async Task<ActionResult<ApiCredentials>> Create(ApiCredentials data)
        {
            _context.ApiCredentials.Add(data);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = data.ApiCredId }, data);
        }

        // PUT: api/apicredentials/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, ApiCredentials data)
        {
            if (id != data.ApiCredId)
                return BadRequest("ID mismatch.");

            var existing = await _context.ApiCredentials.FindAsync(id);
            if (existing == null) return NotFound();

            existing.PartnerId = data.PartnerId;
            existing.ServiceId = data.ServiceId;
            existing.ApiKey = data.ApiKey;
            existing.ApiSecret = data.ApiSecret;
            existing.TokenExpiry = data.TokenExpiry;
            existing.AllowedIp = data.AllowedIp;
            existing.Status = data.Status;
            existing.UpdatedAt = System.DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/apicredentials/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var item = await _context.ApiCredentials.FindAsync(id);
            if (item == null) return NotFound();

            _context.ApiCredentials.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
