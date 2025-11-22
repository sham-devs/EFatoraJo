using ShamDevs.EFatoraJo.Enums;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ShamDevs.EFatoraJo.Models.Responses
{
    public class EInvoiceMessage
    {
        [JsonPropertyName("type")]
        public EInvoiceMessageType Type { get; set; }

        [JsonPropertyName("status")]
        public EInvoiceMessageStatus Status { get; set; }

        [JsonPropertyName("EINV_CODE")]
        public string? Code { get; set; }

        [JsonPropertyName("EINV_CATEGORY")]
        public string? Category { get; set; }

        [JsonPropertyName("EINV_MESSAGE")]
        public string? Message { get; set; }

        public string ToFormattedString()
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(Category)) parts.Add(Category!);
            if (!string.IsNullOrEmpty(Message)) parts.Add(Message!);
            if (!string.IsNullOrEmpty(Code)) parts.Add($"(Code: {Code})");

            return string.Join(": ", parts);
        }
    }
}
