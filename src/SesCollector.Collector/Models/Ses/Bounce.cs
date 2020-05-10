using System;

namespace SesCollector.Collector.Models.Ses
{
    public class Bounce
    {
        public string BounceType { get; set; }
        public string BounceSubType { get; set; }

        public BouncedRecipient[] BouncedRecipients { get; set; }

        public string ReportingMTA { get; set; }

        public DateTime Timestamp { get; set; }

        public string FeedbackId { get; set; }
        public string RemoteMtaIp { get; set; }
    }
    
    public class BouncedRecipient 
    {
        public string Status { get; set; }
        public string Action { get; set; }
        public string DiagnosticCode { get; set; }
        public string EmailAddress { get; set; }
    }
}