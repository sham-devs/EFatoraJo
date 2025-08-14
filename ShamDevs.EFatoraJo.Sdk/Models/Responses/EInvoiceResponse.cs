<<<<<<< HEAD
﻿using System.Text.Json.Serialization;
=======
﻿using Newtonsoft.Json;
>>>>>>> 22f095a040cb70b6315767ed1773569f2609b27d
using ShamDevs.EFatoraJo.Enums;

namespace ShamDevs.EFatoraJo.Models.Responses
{
    public class EInvoiceResponse
    {
<<<<<<< HEAD
        [JsonPropertyName("EINV_RESULTS")]
        public EInvoiceResults Results { get; set; } = new EInvoiceResults();

        [JsonPropertyName("EINV_STATUS")]
        public EInvoiceStatus Status { get; set; }

        [JsonPropertyName("EINV_SINGED_INVOICE")]
        public string? SignedInvoice { get; set; }

        [JsonPropertyName("EINV_QR")]
        public string? Qr { get; set; }

        [JsonPropertyName("EINV_NUM")]
        public string? InvoiceNumber { get; set; }

        [JsonPropertyName("EINV_INV_UUID")]
=======
        [JsonProperty("EINV_RESULTS")]
        public EInvoiceResults Results { get; set; } = new EInvoiceResults();

        [JsonProperty("EINV_STATUS")]
        public EInvoiceStatus Status { get; set; }

        [JsonProperty("EINV_SINGED_INVOICE")]
        public string? SignedInvoice { get; set; }

        [JsonProperty("EINV_QR")]
        public string? Qr { get; set; }

        [JsonProperty("EINV_NUM")]
        public string? InvoiceNumber { get; set; }

        [JsonProperty("EINV_INV_UUID")]
>>>>>>> 22f095a040cb70b6315767ed1773569f2609b27d
        public string? InvoiceUuid { get; set; }

        // Status check methods
        public bool IsSuccessfullySubmitted()
            => Status == EInvoiceStatus.SUBMITTED &&
               Results?.Status == EInvoiceProcessingStatus.PASS;

        public bool IsAlreadySubmitted()
            => Status == EInvoiceStatus.ALREADY_SUBMITTED;

        public bool HasNewUuid()
            => !string.IsNullOrEmpty(InvoiceUuid);

        public bool HasErrors()
            => Results?.Errors?.Count > 0;

        public bool HasWarnings()
            => Results?.Warnings?.Count > 0;

        // Message formatting methods
        public string GetFormattedErrors(int maxToShow = 5)
            => Results?.GetFormattedErrors(maxToShow) ?? "No error details available";

        public string GetFormattedWarnings(int maxToShow = 5)
            => Results?.GetFormattedWarnings(maxToShow) ?? "No warnings available";

        public string GetFormattedInfo(int maxToShow = 5)
            => Results?.GetFormattedInfo(maxToShow) ?? "No info messages available";

        // Combined status message
        public string GetStatusSummary()
        {
            if (IsSuccessfullySubmitted()) return "Invoice successfully submitted";
            if (IsAlreadySubmitted()) return "Invoice was already submitted";
            if (HasErrors()) return $"Invoice submission failed with {Results.Errors.Count} error(s)";

            return "Unknown invoice status";
        }
    }
}
