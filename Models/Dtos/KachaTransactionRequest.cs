namespace GBEMiddlewareApi.Models.Dtos
{
    public class KachaTransactionRequest
    {
        public string UserId { get; set; }
        public string Password { get; set; }
        public string ServiceCode { get; set; }
        public string AccountNo { get; set; }
        public string BranchCode { get; set; }
        public decimal Amount { get; set; }
        public string TxnDate { get; set; }
        public string ExtEntity { get; set; }
        public string ExtRefNo { get; set; }
        public string TxnDesc { get; set; }
    }
}
