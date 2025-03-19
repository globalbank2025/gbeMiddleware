namespace GBEMiddlewareApi.Models
{
    public class VatCollectionTransactionDto
    {
        public int BranchCode { get; set; }
        public string AccountNumber { get; set; }
        public string CustomerVatRegistrationNo { get; set; }
        public string CustomerTinNo { get; set; }
        public string CustomerTelephone { get; set; }
        public decimal PrincipalAmount { get; set; }
        public string ServiceIncomeGl { get; set; }
        public decimal ServiceCharge { get; set; }
        public decimal VatOnServiceCharge { get; set; }
        public decimal TotalAmount { get; set; }
        // The customer name is expected to come in the request body.
        public string CustomerName { get; set; }
    }
}
