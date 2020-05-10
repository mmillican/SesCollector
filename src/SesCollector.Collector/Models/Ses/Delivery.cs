using System;

namespace SesCollector.Collector.Models.Ses
{
    public class Delivery 
    {
        public DateTime Timestamp { get; set; }

        public int ProcessingTimeMillis { get; set; }

        public string[] Recipients { get; set; }

        public string SmtpResponse { get; set; }

        public string ReportingMTA { get; set; }
        public string RemoteMtaIp { get; set; }
    }
}