using System;

namespace GBEMiddlewareApi.Models
{
    public class VatCollectionTransaction
    {
        public int Id { get; set; }

        // Existing fields
        public int BranchCode { get; set; }
        public string AccountNumber { get; set; }
        public string? CustomerVatRegistrationNo { get; set; } // now nullable
        public string? CustomerTinNo { get; set; }               // now nullable
        public string? CustomerTelephone { get; set; }           // now nullable
        public decimal PrincipalAmount { get; set; }
        public string ServiceIncomeGl { get; set; }
        public decimal ServiceCharge { get; set; }
        public decimal VatOnServiceCharge { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }   // e.g., "PENDING", "APPROVED", etc.

        // New fields
        public string? ApprovedBy { get; set; }       // will store the username
        public DateTime? ApprovedDateTime { get; set; }
        public string? CustomerName { get; set; }      // made nullable
        public string? ServiceChargeReference { get; set; }
        // Removed: public string? VatReference { get; set; }
    }
}
