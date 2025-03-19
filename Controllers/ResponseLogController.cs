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
    public class ResponseLogController : ControllerBase
    {
        private readonly MiddlewareDbContext _context;

        public ResponseLogController(MiddlewareDbContext context)
        {
            _context = context;
        }

        // GET: api/responselog
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ResponseLog>>> GetAll()
        {
            return await _context.ResponseLogs.ToListAsync();
        }

        // GET: api/responselog/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseLog>> Get(long id)
        {
            var item = await _context.ResponseLogs.FindAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        // POST: api/responselog
        [HttpPost]
        public async Task<ActionResult<ResponseLog>> Create(ResponseLog data)
        {
            _context.ResponseLogs.Add(data);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = data.ResponseId }, data);
        }

        // PUT: api/responselog/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, ResponseLog data)
        {
            if (id != data.ResponseId)
                return BadRequest("ID mismatch.");

            var existing = await _context.ResponseLogs.FindAsync(id);
            if (existing == null) return NotFound();

            existing.RequestId = data.RequestId;
            existing.ResponseTimestamp = data.ResponseTimestamp;
            existing.HttpStatusCode = data.HttpStatusCode;
            existing.ResponseHeaders = data.ResponseHeaders;
            existing.ResponsePayload = data.ResponsePayload;
            existing.ProcessingTimeMs = data.ProcessingTimeMs;
            existing.Status = data.Status;
            // created_at typically is set once

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/responselog/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var item = await _context.ResponseLogs.FindAsync(id);
            if (item == null) return NotFound();

            _context.ResponseLogs.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
