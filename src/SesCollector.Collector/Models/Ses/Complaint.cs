using System;

namespace SesCollector.Collector.Models.Ses
{
    public class Complaint
    {
        public string UserAgent { get; set; }
        public SesComplainedRecipient[] ComplainedRecipients { get; set; }

        public string ComplaintFeedbackType { get; set; }
        public DateTime ArrivalDate { get; set; }

        public string FeedbackId { get; set; }
    }
    

    public class SesComplainedRecipient 
    {
        public string EmailAddress { get; set; }
    }
}