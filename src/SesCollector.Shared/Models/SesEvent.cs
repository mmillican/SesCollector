using System;
using System.Collections.Generic;

namespace SesCollector.Shared.Models
{
    public class SesEvent
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } 

        public string EventType { get; set; }
        public string SubType { get; set; }

        public string FeedbackId { get; set; }
        public string MessageId { get; set; }

        public string Source { get; set; }
        public string SourceArn { get; set; }
        public string SourceIp { get; set; }
        public string SendingAccountId { get; set; }

        public string FromAddress { get; set; }
        public string Subject { get; set; }
        public string MessageDate { get; set; }
        public List<string> Recipients { get; set; } = new List<string>();

        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}