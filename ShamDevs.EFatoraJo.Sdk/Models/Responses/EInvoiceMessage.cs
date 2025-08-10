using Newtonsoft.Json;
using ShamDevs.EFatoraJo.Enums;
using System.Collections.Generic;

namespace ShamDevs.EFatoraJo.Models.Responses
{
    public class EInvoiceMessage
    {
        [JsonProperty("type")]
        public EInvoiceMessageType Type { get; set; }

        [JsonProperty("status")]
        public EInvoiceMessageStatus Status { get; set; }

        [JsonProperty("EINV_CODE")]
        public string? Code { get; set; }

        [JsonProperty("EINV_CATEGORY")]
        public string? Category { get; set; }

        [JsonProperty("EINV_MESSAGE")]
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
