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
    public class MappingRuleController : ControllerBase
    {
        private readonly MiddlewareDbContext _context;

        public MappingRuleController(MiddlewareDbContext context)
        {
            _context = context;
        }

        // GET: api/mappingrule
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MappingRule>>> GetAll()
        {
            return await _context.MappingRules.ToListAsync();
        }

        // GET: api/mappingrule/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MappingRule>> Get(long id)
        {
            var item = await _context.MappingRules.FindAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        // POST: api/mappingrule
        [HttpPost]
        public async Task<ActionResult<MappingRule>> Create(MappingRule data)
        {
            _context.MappingRules.Add(data);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = data.MappingId }, data);
        }

        // PUT: api/mappingrule/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, MappingRule data)
        {
            if (id != data.MappingId)
                return BadRequest("ID mismatch.");

            var existing = await _context.MappingRules.FindAsync(id);
            if (existing == null) return NotFound();

            existing.ServiceId = data.ServiceId;
            existing.SourceFormat = data.SourceFormat;
            existing.TargetFormat = data.TargetFormat;
            existing.MappingRules = data.MappingRules;
            existing.Direction = data.Direction;
            existing.UpdatedAt = System.DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/mappingrule/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var item = await _context.MappingRules.FindAsync(id);
            if (item == null) return NotFound();

            _context.MappingRules.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
