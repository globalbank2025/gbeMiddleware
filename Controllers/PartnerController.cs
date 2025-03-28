using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

using GBEMiddlewareApi.Data;
using GBEMiddlewareApi.Models;

namespace GBEMiddlewareApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartnerController : ControllerBase
    {
        // -----------------------------------------------------------------
        // 1. CreatePartnerDto: used only for POST requests (no PartnerId).
        // -----------------------------------------------------------------
        public class CreatePartnerDto
        {
            public string PartnerCode { get; set; }
            public string PartnerName { get; set; }
            public string ContactPerson { get; set; }
            public string ContactEmail { get; set; }
            public string ContactPhone { get; set; }
            public string Status { get; set; } = "ACTIVE";
        }

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
        public async Task<IActionResult> GetPartner(long id)
        {
            var partner = await _context.Partners.FindAsync(id);
            if (partner == null)
                return NotFound(new { message = "Partner not found." });

            return Ok(partner);
        }

        // -----------------------------------------------------------------
        // POST: api/partner
        // Accepts CreatePartnerDto. No PartnerId => no empty-string problem.
        // -----------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> CreatePartner([FromBody] CreatePartnerDto dto)
        {
            // 1) Validate model state
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Invalid data.",
                    errors = ModelState
                });
            }

            // 2) Create the Partner entity
            var partner = new Partner
            {
                PartnerCode = dto.PartnerCode,
                PartnerName = dto.PartnerName,
                ContactPerson = dto.ContactPerson,
                ContactEmail = dto.ContactEmail,
                ContactPhone = dto.ContactPhone,
                Status = dto.Status,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.Partners.Add(partner);
            await _context.SaveChangesAsync();

            // 3) Return a Created response with a success message
            return CreatedAtAction(
                nameof(GetPartner),
                new { id = partner.PartnerId },
                new { message = "Partner created successfully", data = partner }
            );
        }

        // PUT: api/partner/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePartner(long id, [FromBody] Partner partnerData)
        {
            // Ensure the route ID matches the body ID
            if (id != partnerData.PartnerId)
                return BadRequest(new { message = "Partner ID mismatch." });

            var existingPartner = await _context.Partners.FindAsync(id);
            if (existingPartner == null)
                return NotFound(new { message = "Partner not found." });

            // Update fields
            existingPartner.PartnerCode = partnerData.PartnerCode;
            existingPartner.PartnerName = partnerData.PartnerName;
            existingPartner.ContactPerson = partnerData.ContactPerson;
            existingPartner.ContactEmail = partnerData.ContactEmail;
            existingPartner.ContactPhone = partnerData.ContactPhone;
            existingPartner.Status = partnerData.Status;
            existingPartner.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Partner updated successfully" });
        }

        // DELETE: api/partner/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePartner(long id)
        {
            var partner = await _context.Partners.FindAsync(id);
            if (partner == null)
                return NotFound(new { message = "Partner not found." });

            _context.Partners.Remove(partner);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Partner deleted successfully" });
        }
    }
}
