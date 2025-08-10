namespace ShamDevs.EFatoraJo.Exceptions
{
    public class EInvoiceApiException : EInvoiceException
    {
        public int StatusCode { get; }
        public string ResponseContent { get; }

        public EInvoiceApiException(int statusCode, string response)
            : base($"API request failed with status {statusCode}")
        {
            StatusCode = statusCode;
            ResponseContent = response;
        }
    }

}
