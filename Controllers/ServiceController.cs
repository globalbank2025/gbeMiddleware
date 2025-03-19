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
                return NotFound();

            return service;
        }

        // POST: api/service
        [HttpPost]
        public async Task<ActionResult<Service>> CreateService(Service service)
        {
            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetService), new { id = service.ServiceId }, service);
        }

        // PUT: api/service/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(long id, Service serviceData)
        {
            if (id != serviceData.ServiceId)
                return BadRequest("Service ID mismatch.");

            var existingService = await _context.Services.FindAsync(id);
            if (existingService == null)
                return NotFound();

            existingService.ServiceCode = serviceData.ServiceCode;
            existingService.ServiceName = serviceData.ServiceName;
            existingService.Description = serviceData.Description;
            existingService.Status = serviceData.Status;
            existingService.UpdatedAt = System.DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/service/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteService(long id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
                return NotFound();

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
