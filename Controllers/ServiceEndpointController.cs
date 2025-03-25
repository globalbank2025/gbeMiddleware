using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using GBEMiddlewareApi.Data;
using GBEMiddlewareApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace GBEMiddlewareApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requires authentication for all endpoints
    public class ServiceEndpointController : ControllerBase
    {
        private readonly MiddlewareDbContext _context;

        public ServiceEndpointController(MiddlewareDbContext context)
        {
            _context = context;
        }

        // GET: api/serviceendpoint
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceEndpoint>>> GetAll()
        {
            return await _context.ServiceEndpoints.ToListAsync();
        }

        // GET: api/serviceendpoint/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceEndpoint>> Get(long id)
        {
            var item = await _context.ServiceEndpoints.FindAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        // POST: api/serviceendpoint
        [HttpPost]
        public async Task<ActionResult<ServiceEndpoint>> Create(ServiceEndpoint data)
        {
            _context.ServiceEndpoints.Add(data);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = data.EndpointId }, data);
        }

        // PUT: api/serviceendpoint/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, ServiceEndpoint data)
        {
            if (id != data.EndpointId)
                return BadRequest("ID mismatch.");

            var existing = await _context.ServiceEndpoints.FindAsync(id);
            if (existing == null) return NotFound();

            existing.ServiceId = data.ServiceId;
            existing.Environment = data.Environment;
            existing.EndpointUrl = data.EndpointUrl;
            existing.HttpMethod = data.HttpMethod;
            existing.SoapAction = data.SoapAction;
            existing.ConnectionTimeout = data.ConnectionTimeout;
            existing.ReadTimeout = data.ReadTimeout;
            existing.Status = data.Status;
            existing.UpdatedAt = System.DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/serviceendpoint/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var item = await _context.ServiceEndpoints.FindAsync(id);
            if (item == null) return NotFound();

            _context.ServiceEndpoints.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
