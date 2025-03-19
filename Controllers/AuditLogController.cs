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
    public class AuditLogController : ControllerBase
    {
        private readonly MiddlewareDbContext _context;

        public AuditLogController(MiddlewareDbContext context)
        {
            _context = context;
        }

        // GET: api/auditlog
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuditLog>>> GetAll()
        {
            return await _context.AuditLogs.ToListAsync();
        }

        // GET: api/auditlog/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AuditLog>> Get(long id)
        {
            var item = await _context.AuditLogs.FindAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        // POST: api/auditlog
        [HttpPost]
        public async Task<ActionResult<AuditLog>> Create(AuditLog data)
        {
            _context.AuditLogs.Add(data);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = data.AuditId }, data);
        }

        // PUT: api/auditlog/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, AuditLog data)
        {
            if (id != data.AuditId)
                return BadRequest("ID mismatch.");

            var existing = await _context.AuditLogs.FindAsync(id);
            if (existing == null) return NotFound();

            existing.TableName = data.TableName;
            existing.RecordId = data.RecordId;
            existing.Operation = data.Operation;
            existing.OldData = data.OldData;
            existing.NewData = data.NewData;
            existing.ChangedBy = data.ChangedBy;
            existing.ChangedAt = data.ChangedAt;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/auditlog/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var item = await _context.AuditLogs.FindAsync(id);
            if (item == null) return NotFound();

            _context.AuditLogs.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
