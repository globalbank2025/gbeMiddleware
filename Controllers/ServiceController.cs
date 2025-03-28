using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using GBEMiddlewareApi.Data;
using GBEMiddlewareApi.Models;

namespace GBEMiddlewareApi.Controllers
{
    // DTO for service creation (no ServiceId)
    public class CreateServiceDto
    {
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public string ServiceType { get; set; }
        public string OffsetAccNo { get; set; }
        public string Status { get; set; } = "ACTIVE";
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ServiceController : ControllerBase
    {
        private readonly MiddlewareDbContext _context;

        public ServiceController(MiddlewareDbContext context)
        {
            _context = context;
        }

        // GET: api/service
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Service>>> GetAllServices()
        {
            return await _context.Services.ToListAsync();
        }

        // GET: api/service/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Service>> GetService(long id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
                return NotFound(new { message = "Service not found." });

            return Ok(service);
        }

        // POST: api/service
        [HttpPost]
        public async Task<ActionResult<Service>> CreateService([FromBody] CreateServiceDto dto)
        {
            // Check for duplicate service code or service name
            var duplicateService = await _context.Services.FirstOrDefaultAsync(s =>
                s.ServiceCode == dto.ServiceCode || s.ServiceName == dto.ServiceName);
            if (duplicateService != null)
            {
                return BadRequest(new { message = "A service with the same service code or service name already exists." });
            }

            // Map DTO to Service entity
            var service = new Service
            {
                ServiceCode = dto.ServiceCode,
                ServiceName = dto.ServiceName,
                Description = dto.Description,
                ServiceType = dto.ServiceType,
                OffsetAccNo = dto.OffsetAccNo,
                Status = dto.Status,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetService), new { id = service.ServiceId }, service);
        }

        // PUT: api/service/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(long id, [FromBody] Service serviceData)
        {
            if (id != serviceData.ServiceId)
                return BadRequest(new { message = "Service ID mismatch." });

            var existingService = await _context.Services.FindAsync(id);
            if (existingService == null)
                return NotFound(new { message = "Service not found." });

            // Check for duplicate service code or service name in other records
            var duplicateService = await _context.Services.FirstOrDefaultAsync(s =>
                (s.ServiceCode == serviceData.ServiceCode || s.ServiceName == serviceData.ServiceName) &&
                s.ServiceId != id);
            if (duplicateService != null)
            {
                return BadRequest(new { message = "A service with the same service code or service name already exists." });
            }

            // Update properties
            existingService.ServiceCode = serviceData.ServiceCode;
            existingService.ServiceName = serviceData.ServiceName;
            existingService.Description = serviceData.Description;
            existingService.ServiceType = serviceData.ServiceType;
            existingService.OffsetAccNo = serviceData.OffsetAccNo;
            existingService.Status = serviceData.Status;
            existingService.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Service updated successfully." });
        }

        // DELETE: api/service/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteService(long id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
                return NotFound(new { message = "Service not found." });

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Service deleted successfully." });
        }
    }
}
