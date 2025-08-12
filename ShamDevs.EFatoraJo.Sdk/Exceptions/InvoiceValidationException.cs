using System.Collections.Generic;

namespace ShamDevs.EFatoraJo.Exceptions
{
    public class InvoiceValidationException : EInvoiceException
    {
        public List<string> ValidationErrors { get; }

        public InvoiceValidationException(List<string> errors)
            : base($"Invoice validation failed: {string.Join(", ", errors)}")
        {
            ValidationErrors = errors;
        }
    }
}
