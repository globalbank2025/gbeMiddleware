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
    public class RequestLogController : ControllerBase
    {
        private readonly MiddlewareDbContext _context;

        public RequestLogController(MiddlewareDbContext context)
        {
            _context = context;
        }

        // GET: api/requestlog
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequestLog>>> GetAll()
        {
            return await _context.RequestLogs.ToListAsync();
        }

        // GET: api/requestlog/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RequestLog>> Get(long id)
        {
            var item = await _context.RequestLogs.FindAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        // POST: api/requestlog
        [HttpPost]
        public async Task<ActionResult<RequestLog>> Create(RequestLog data)
        {
            _context.RequestLogs.Add(data);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = data.RequestId }, data);
        }

        // PUT: api/requestlog/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, RequestLog data)
        {
            if (id != data.RequestId)
                return BadRequest("ID mismatch.");

            var existing = await _context.RequestLogs.FindAsync(id);
            if (existing == null) return NotFound();

            existing.PartnerId = data.PartnerId;
            existing.ServiceId = data.ServiceId;
            existing.RequestTimestamp = data.RequestTimestamp;
            existing.HttpMethod = data.HttpMethod;
            existing.RequestHeaders = data.RequestHeaders;
            existing.RequestPayload = data.RequestPayload;
            existing.ClientIp = data.ClientIp;
            existing.CorrelationId = data.CorrelationId;
            existing.Status = data.Status;
            // created_at is typically set once, so we usually don't update it

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/requestlog/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var item = await _context.RequestLogs.FindAsync(id);
            if (item == null) return NotFound();

            _context.RequestLogs.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
