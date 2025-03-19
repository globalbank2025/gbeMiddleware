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
    public class PartnerController : ControllerBase
    {
        private readonly MiddlewareDbContext _context;

        public PartnerController(MiddlewareDbContext context)
        {
            _context = context;
        }

        // GET: api/partner
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Partner>>> GetAllPartners()
        {
            return await _context.Partners.ToListAsync();
        }

        // GET: api/partner/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Partner>> GetPartner(long id)
        {
            var partner = await _context.Partners.FindAsync(id);
            if (partner == null)
                return NotFound();

            return partner;
        }

        // POST: api/partner
        [HttpPost]
        public async Task<ActionResult<Partner>> CreatePartner(Partner partner)
        {
            _context.Partners.Add(partner);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPartner), new { id = partner.PartnerId }, partner);
        }

        // PUT: api/partner/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePartner(long id, Partner partnerData)
        {
            if (id != partnerData.PartnerId)
                return BadRequest("Partner ID mismatch.");

            var existingPartner = await _context.Partners.FindAsync(id);
            if (existingPartner == null)
                return NotFound();

            // Update fields
            existingPartner.PartnerCode = partnerData.PartnerCode;
            existingPartner.PartnerName = partnerData.PartnerName;
            existingPartner.ContactPerson = partnerData.ContactPerson;
            existingPartner.ContactEmail = partnerData.ContactEmail;
            existingPartner.ContactPhone = partnerData.ContactPhone;
            existingPartner.Status = partnerData.Status;
            existingPartner.UpdatedAt = System.DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/partner/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePartner(long id)
        {
            var partner = await _context.Partners.FindAsync(id);
            if (partner == null)
                return NotFound();

            _context.Partners.Remove(partner);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
