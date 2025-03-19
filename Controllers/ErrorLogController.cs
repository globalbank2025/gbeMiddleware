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
    public class ErrorLogController : ControllerBase
    {
        private readonly MiddlewareDbContext _context;

        public ErrorLogController(MiddlewareDbContext context)
        {
            _context = context;
        }

        // GET: api/errorlog
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ErrorLog>>> GetAll()
        {
            return await _context.ErrorLogs.ToListAsync();
        }

        // GET: api/errorlog/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ErrorLog>> Get(long id)
        {
            var item = await _context.ErrorLogs.FindAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        // POST: api/errorlog
        [HttpPost]
        public async Task<ActionResult<ErrorLog>> Create(ErrorLog data)
        {
            _context.ErrorLogs.Add(data);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = data.ErrorId }, data);
        }

        // PUT: api/errorlog/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, ErrorLog data)
        {
            if (id != data.ErrorId)
                return BadRequest("ID mismatch.");

            var existing = await _context.ErrorLogs.FindAsync(id);
            if (existing == null) return NotFound();

            existing.TransactionId = data.TransactionId;
            existing.RequestId = data.RequestId;
            existing.ErrorTimestamp = data.ErrorTimestamp;
            existing.ErrorSeverity = data.ErrorSeverity;
            existing.ErrorCode = data.ErrorCode;
            existing.ErrorMessage = data.ErrorMessage;
            existing.StackTrace = data.StackTrace;
            existing.Status = data.Status;
            // created_at is set once

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/errorlog/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var item = await _context.ErrorLogs.FindAsync(id);
            if (item == null) return NotFound();

            _context.ErrorLogs.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
