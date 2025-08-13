using System;

namespace ShamDevs.EFatoraJo.Exceptions
{
    public class UblGenerationException : EInvoiceException
    {
        public UblGenerationException(string message) : base(message) { }
        public UblGenerationException(string message, Exception inner) : base(message, inner) { }
    }

}
