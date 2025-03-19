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
    public class ScheduledTaskController : ControllerBase
    {
        private readonly MiddlewareDbContext _context;

        public ScheduledTaskController(MiddlewareDbContext context)
        {
            _context = context;
        }

        // GET: api/scheduledtask
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ScheduledTask>>> GetAll()
        {
            return await _context.ScheduledTasks.ToListAsync();
        }

        // GET: api/scheduledtask/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ScheduledTask>> Get(long id)
        {
            var item = await _context.ScheduledTasks.FindAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        // POST: api/scheduledtask
        [HttpPost]
        public async Task<ActionResult<ScheduledTask>> Create(ScheduledTask data)
        {
            _context.ScheduledTasks.Add(data);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = data.TaskId }, data);
        }

        // PUT: api/scheduledtask/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, ScheduledTask data)
        {
            if (id != data.TaskId)
                return BadRequest("ID mismatch.");

            var existing = await _context.ScheduledTasks.FindAsync(id);
            if (existing == null) return NotFound();

            existing.TaskName = data.TaskName;
            existing.CronExpression = data.CronExpression;
            existing.LastRunTime = data.LastRunTime;
            existing.NextRunTime = data.NextRunTime;
            existing.Status = data.Status;
            existing.UpdatedAt = System.DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/scheduledtask/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var item = await _context.ScheduledTasks.FindAsync(id);
            if (item == null) return NotFound();

            _context.ScheduledTasks.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
