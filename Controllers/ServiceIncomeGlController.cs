using Microsoft.AspNetCore.Mvc;
using GBEMiddlewareApi.Data;
using GBEMiddlewareApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GBEMiddlewareApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceIncomeGlController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServiceIncomeGlController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/serviceincomegl
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _context.ServiceIncomeGls.ToListAsync();
            return Ok(items);
        }

        // GET: api/serviceincomegl/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await _context.ServiceIncomeGls.FindAsync(id);
            if (item == null)
                return NotFound();

            return Ok(item);
        }

        // POST: api/serviceincomegl
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ServiceIncomeGl model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.ServiceIncomeGls.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        // PUT: api/serviceincomegl/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ServiceIncomeGl model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existing = await _context.ServiceIncomeGls.FindAsync(id);
            if (existing == null)
                return NotFound();

            // Update all properties including the new ones
            existing.GlCode = model.GlCode;
            existing.Name = model.Name;
            existing.Description = model.Description;
            existing.Status = model.Status;
            existing.CalculationType = model.CalculationType;
            existing.FlatPrice = model.FlatPrice;
            existing.Rate = model.Rate;

            _context.ServiceIncomeGls.Update(existing);
            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        // DELETE: api/serviceincomegl/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.ServiceIncomeGls.FindAsync(id);
            if (item == null)
                return NotFound();

            _context.ServiceIncomeGls.Remove(item);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
