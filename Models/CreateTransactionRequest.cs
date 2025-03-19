namespace GBEMiddlewareApi.Models
{
    public class CreateTransactionRequest
    {
        // Some fields can be defaulted if they're always the same
        public string TxnBrn { get; set; }      // e.g. "164" for TXNBRN
        public string TxnAcc { get; set; }      // e.g. "1647315522361"
        public decimal TxnAmt { get; set; }     // e.g. 255
        public string OffsetBrn { get; set; }   // e.g. "124"
        public string OffsetAcc { get; set; }   // e.g. "1241101535121"
        public string TxnDate { get; set; }     // e.g. "2025-02-17"
        public string ValDate { get; set; }     // e.g. "2025-02-17"
        public string Narrative { get; set; }   // e.g. "Biniyam Test"
    }
}
