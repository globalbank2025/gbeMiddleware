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
    public class TransactionController : ControllerBase
    {
        private readonly MiddlewareDbContext _context;

        public TransactionController(MiddlewareDbContext context)
        {
            _context = context;
        }

        // GET: api/transaction
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetAll()
        {
            return await _context.Transactions.ToListAsync();
        }

        // GET: api/transaction/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Transaction>> Get(long id)
        {
            var item = await _context.Transactions.FindAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        // POST: api/transaction
        [HttpPost]
        public async Task<ActionResult<Transaction>> Create(Transaction data)
        {
            _context.Transactions.Add(data);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = data.TransactionId }, data);
        }

        // PUT: api/transaction/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, Transaction data)
        {
            if (id != data.TransactionId)
                return BadRequest("ID mismatch.");

            var existing = await _context.Transactions.FindAsync(id);
            if (existing == null) return NotFound();

            existing.RequestId = data.RequestId;
            existing.PartnerId = data.PartnerId;
            existing.ServiceId = data.ServiceId;
            existing.TransactionRef = data.TransactionRef;
            existing.TransactionType = data.TransactionType;
            existing.Amount = data.Amount;
            existing.Currency = data.Currency;
            existing.TransactionStatus = data.TransactionStatus;
            existing.UpdatedAt = System.DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/transaction/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var item = await _context.Transactions.FindAsync(id);
            if (item == null) return NotFound();

            _context.Transactions.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
