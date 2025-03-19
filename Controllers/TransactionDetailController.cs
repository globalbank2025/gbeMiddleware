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
    public class TransactionDetailController : ControllerBase
    {
        private readonly MiddlewareDbContext _context;

        public TransactionDetailController(MiddlewareDbContext context)
        {
            _context = context;
        }

        // GET: api/transactiondetail
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionDetail>>> GetAll()
        {
            return await _context.TransactionDetails.ToListAsync();
        }

        // GET: api/transactiondetail/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionDetail>> Get(long id)
        {
            var item = await _context.TransactionDetails.FindAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        // POST: api/transactiondetail
        [HttpPost]
        public async Task<ActionResult<TransactionDetail>> Create(TransactionDetail data)
        {
            _context.TransactionDetails.Add(data);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = data.TxDetailId }, data);
        }

        // PUT: api/transactiondetail/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, TransactionDetail data)
        {
            if (id != data.TxDetailId)
                return BadRequest("ID mismatch.");

            var existing = await _context.TransactionDetails.FindAsync(id);
            if (existing == null) return NotFound();

            existing.TransactionId = data.TransactionId;
            existing.StepSequence = data.StepSequence;
            existing.StepName = data.StepName;
            existing.RequestPayload = data.RequestPayload;
            existing.ResponsePayload = data.ResponsePayload;
            existing.Status = data.Status;
            existing.StartedAt = data.StartedAt;
            existing.EndedAt = data.EndedAt;
            existing.ErrorMessage = data.ErrorMessage;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/transactiondetail/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var item = await _context.TransactionDetails.FindAsync(id);
            if (item == null) return NotFound();

            _context.TransactionDetails.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
