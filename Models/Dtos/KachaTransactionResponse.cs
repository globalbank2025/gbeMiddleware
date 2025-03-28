namespace GBEMiddlewareApi.Models.Dtos
{
    public class KachaTransactionResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string BankRefNo { get; set; }
        public string ErrorMsg { get; set; }
    }
}
