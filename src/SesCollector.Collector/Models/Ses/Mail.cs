using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SesCollector.Collector.Models.Ses
{
    public class SesMail
    {
        public DateTime Timestamp { get; set; }

        public string Source { get; set; }
        
        public string SourceArn { get; set; }

        public string SourceIp { get; set; }
        public string SendingAccountId { get; set; }

        public List<string> Destination { get; set; }

        public bool HeadersTruncated { get; set; }

        // public Dictionary<string, string> Headers { get; set; }
        public MailHeaders CommonHeaders { get; set; }
    }
    
    public class MailHeaders
    {
        public List<string> From { get; set; }

        public string Date { get; set; }

        public List<string> To { get; set; }
        
        public string MessageId { get; set; }
        
        public string Subject { get; set; }
    }
}