<<<<<<< HEAD
﻿using ShamDevs.EFatoraJo.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
=======
﻿using Newtonsoft.Json;
using ShamDevs.EFatoraJo.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
>>>>>>> 22f095a040cb70b6315767ed1773569f2609b27d

namespace ShamDevs.EFatoraJo.Models.Responses
{
    public class EInvoiceResults
    {
<<<<<<< HEAD
        [JsonPropertyName("status")]
        public EInvoiceProcessingStatus Status { get; set; }

        [JsonPropertyName("INFO")]
        public List<EInvoiceMessage> Info { get; set; } = new List<EInvoiceMessage>();

        [JsonPropertyName("WARNINGS")]
        public List<EInvoiceMessage> Warnings { get; set; } = new List<EInvoiceMessage>();

        [JsonPropertyName("ERRORS")]
=======
        [JsonProperty("status")]
        public EInvoiceProcessingStatus Status { get; set; }

        [JsonProperty("INFO")]
        public List<EInvoiceMessage> Info { get; set; } = new List<EInvoiceMessage>();

        [JsonProperty("WARNINGS")]
        public List<EInvoiceMessage> Warnings { get; set; } = new List<EInvoiceMessage>();

        [JsonProperty("ERRORS")]
>>>>>>> 22f095a040cb70b6315767ed1773569f2609b27d
        public List<EInvoiceMessage> Errors { get; set; } = new List<EInvoiceMessage>();

        public string GetFormattedErrors(int maxToShow = 5)
            => FormatMessages(Errors, "errors", maxToShow);

        public string GetFormattedWarnings(int maxToShow = 5)
            => FormatMessages(Warnings, "warnings", maxToShow);

        public string GetFormattedInfo(int maxToShow = 5)
            => FormatMessages(Info, "info messages", maxToShow);

        private string FormatMessages(List<EInvoiceMessage> messages, string messageType, int maxToShow)
        {
            if (messages == null || messages.Count == 0)
                return $"No {messageType} found";

            var messageList = messages.Select(m => m.ToFormattedString()).ToList();

            if (messageList.Count <= maxToShow)
            {
                return string.Join(Environment.NewLine, messageList);
            }

            return string.Join(Environment.NewLine, messageList.Take(maxToShow)) +
                   $"{Environment.NewLine}...and {messageList.Count - maxToShow} more {messageType}";
        }
    }
}
