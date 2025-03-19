namespace GBEMiddlewareApi.Models
{
    public class QueryCustomerAccountRequest
    {
        // Branch number, e.g., "124"
        public string BRN { get; set; }

        // Account number, e.g., "1241101535121"
        public string ACC { get; set; }
    }
}
