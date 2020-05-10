using System.Text.Json.Serialization;

namespace SesCollector.Collector.Models.Ses
{
    public class SesNotification
    {
        public string NotificationType { get; set; }

        public SesMail Mail { get; set; }

        public Bounce Bounce { get; set; }

        // public Complaint Complaint { get; set; }

        // public Delivery Delivery { get; set; }
    }
}