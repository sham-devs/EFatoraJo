<<<<<<< HEAD
﻿using ShamDevs.EFatoraJo.Enums;
using System.Collections.Generic;
using System.Text.Json.Serialization;
=======
﻿using Newtonsoft.Json;
using ShamDevs.EFatoraJo.Enums;
using System.Collections.Generic;
>>>>>>> 22f095a040cb70b6315767ed1773569f2609b27d

namespace ShamDevs.EFatoraJo.Models.Responses
{
    public class EInvoiceMessage
    {
<<<<<<< HEAD
        [JsonPropertyName("type")]
        public EInvoiceMessageType Type { get; set; }

        [JsonPropertyName("status")]
        public EInvoiceMessageStatus Status { get; set; }

        [JsonPropertyName("EINV_CODE")]
        public string? Code { get; set; }

        [JsonPropertyName("EINV_CATEGORY")]
        public string? Category { get; set; }

        [JsonPropertyName("EINV_MESSAGE")]
=======
        [JsonProperty("type")]
        public EInvoiceMessageType Type { get; set; }

        [JsonProperty("status")]
        public EInvoiceMessageStatus Status { get; set; }

        [JsonProperty("EINV_CODE")]
        public string? Code { get; set; }

        [JsonProperty("EINV_CATEGORY")]
        public string? Category { get; set; }

        [JsonProperty("EINV_MESSAGE")]
>>>>>>> 22f095a040cb70b6315767ed1773569f2609b27d
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
