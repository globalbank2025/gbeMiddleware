using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using GBEMiddlewareApi.Data;
using GBEMiddlewareApi.Models;

namespace GBEMiddlewareApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiCredentialsController : ControllerBase
    {
        // -------------------
        // 1) Create DTO
        // -------------------
        // This DTO is used only for creation (POST). It omits ApiCredId so we never pass null.
        public class CreateApiCredentialsDto
        {
            public long PartnerId { get; set; }
            public long ServiceId { get; set; }

            public string ApiKey { get; set; }
            public string ApiSecret { get; set; }

            public string Username { get; set; }
            public string Password { get; set; }

            public DateTimeOffset? TokenExpiry { get; set; }
            public string AllowedIp { get; set; }
            public string Status { get; set; } = "ACTIVE";
        }

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
        public async Task<IActionResult> Get(long id)
        {
            var item = await _context.ApiCredentials.FindAsync(id);
            if (item == null)
                return NotFound(new { message = "API Credentials not found." });

            return Ok(item);
        }

        // -------------------------------------------------------
        // POST: api/apicredentials (CREATE - uses the DTO above)
        // -------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateApiCredentialsDto dto)
        {
            // 1. Validate the request body
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Invalid data.",
                    errors = ModelState
                });
            }

            // 2. Check Partner & Service exist
            var partner = await _context.Partners.FindAsync(dto.PartnerId);
            if (partner == null)
                return BadRequest(new { message = $"Invalid Partner ID: {dto.PartnerId}" });

            var service = await _context.Services.FindAsync(dto.ServiceId);
            if (service == null)
                return BadRequest(new { message = $"Invalid Service ID: {dto.ServiceId}" });

            // 3. Prevent duplicate Partner–Service
            var existingCredential = await _context.ApiCredentials
                .FirstOrDefaultAsync(ac => ac.PartnerId == dto.PartnerId && ac.ServiceId == dto.ServiceId);
            if (existingCredential != null)
            {
                return BadRequest(new { message = "API Credentials for this Partner and Service already exist." });
            }

            // 4. Create the new record
            var newCred = new ApiCredentials
            {
                PartnerId = dto.PartnerId,
                ServiceId = dto.ServiceId,
                ApiKey = dto.ApiKey,
                ApiSecret = dto.ApiSecret,
                Username = dto.Username,
                Password = dto.Password,
                TokenExpiry = dto.TokenExpiry,
                AllowedIp = dto.AllowedIp,
                Status = dto.Status,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                Partner = partner,
                Service = service
            };

            _context.ApiCredentials.Add(newCred);
            await _context.SaveChangesAsync();

            // Return a Created response with a success message
            return CreatedAtAction(nameof(Get),
                new { id = newCred.ApiCredId },
                new { message = "API Credentials created successfully", data = newCred });
        }

        // -----------------------------------------------------
        // PUT: api/apicredentials/5 (UPDATE - uses ApiCredentials model)
        // -----------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] ApiCredentials data)
        {
            if (id != data.ApiCredId)
                return BadRequest(new { message = "ID mismatch." });

            var existing = await _context.ApiCredentials.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "API Credentials not found." });

            // If user changes Partner or Service, ensure no duplicate
            if ((existing.PartnerId != data.PartnerId || existing.ServiceId != data.ServiceId)
                && await _context.ApiCredentials.AnyAsync(ac => ac.PartnerId == data.PartnerId && ac.ServiceId == data.ServiceId))
            {
                return BadRequest(new { message = "API Credentials for this Partner and Service already exist." });
            }

            // Validate new Partner & Service
            var partner = await _context.Partners.FindAsync(data.PartnerId);
            if (partner == null)
                return BadRequest(new { message = $"Invalid Partner ID: {data.PartnerId}" });

            var service = await _context.Services.FindAsync(data.ServiceId);
            if (service == null)
                return BadRequest(new { message = $"Invalid Service ID: {data.ServiceId}" });

            // Perform update
            existing.PartnerId = data.PartnerId;
            existing.ServiceId = data.ServiceId;
            existing.ApiKey = data.ApiKey;
            existing.ApiSecret = data.ApiSecret;
            existing.Username = data.Username;
            existing.Password = data.Password;
            existing.TokenExpiry = data.TokenExpiry;
            existing.AllowedIp = data.AllowedIp;
            existing.Status = data.Status;
            existing.UpdatedAt = DateTimeOffset.UtcNow;

            existing.Partner = partner;
            existing.Service = service;

            await _context.SaveChangesAsync();
            return Ok(new { message = "API Credentials updated successfully" });
        }

        // DELETE: api/apicredentials/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var item = await _context.ApiCredentials.FindAsync(id);
            if (item == null)
                return NotFound(new { message = "API Credentials not found." });

            _context.ApiCredentials.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(new { message = "API Credentials deleted successfully" });
        }
    }
}
