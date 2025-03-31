using System;

namespace GBEMiddlewareApi.Models
{
    public class TransactionLog
    {
        public int Id { get; set; }                          // Primary Key

        // Link this log entry to the VatCollectionTransaction
        public int? VatCollectionTransactionId { get; set; }   // Foreign Key to VatCollectionTransaction

        // Basic transaction info to store
        public string CustomerAccount { get; set; }           // pending.AccountNumber
        public decimal TransactionAmount { get; set; }        // Could store service charge or total – choose what's best
        public string? CustomerName { get; set; }              // pending.CustomerName

        public string RequestPayload { get; set; }            // The SOAP request JSON or a concise snippet
        public string ResponsePayload { get; set; }           // The SOAP response or error message

        public string? TransactionReference { get; set; } // <-- Make it nullable // The reference from SOAP => ref1
        public string? ExternalTransactionReference { get; set; } // <-- Make it nullable // The reference from SOAP => ref1

        //  public string TransactionReference { get; set; }      // The reference from SOAP => ref1
        public string ApprovedBy { get; set; }                // The user who approved (pending.ApprovedBy)
        public DateTimeOffset ApprovedAt { get; set; }        // The date/time of approval

        // Additional metadata
        public DateTimeOffset CreatedAt { get; set; }         // When the log record was created
    }
}
