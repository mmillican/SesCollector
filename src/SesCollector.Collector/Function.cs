using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using SesCollector.Collector.Models;
using SesCollector.Collector.Models.Ses;
using SesCollector.Shared.Models;
using SesCollector.Shared.Services;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SesCollector.Collector
{
    public class Function
    {
        private readonly string [] RelevantNotificationTypes = new[] { "Bounce", "Complaint", "Delivery" };

        private readonly IDynamoContext _dbContext;

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {
            _dbContext = new DynamoDbContext(new AmazonDynamoDBClient());
        }


        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SNS event object and can be used 
        /// to respond to SNS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SNSEvent evnt, ILambdaContext context)
        {
            context.Logger.LogLine($"Received new event with {evnt.Records.Count} records.");

            foreach(var record in evnt.Records)
            {
                await ProcessRecordAsync(record, context);
            }
        }

        public SesEvent ParseRecord(SNSEvent.SNSRecord record, ILambdaContext context)
        {
            context.Logger.LogLine($"Parsing record for {record.Sns.Type} from source {record.EventSource}.");
            context.Logger.LogLine($"--> Message: {record.Sns.Message}");
            
            var notification = JsonSerializer.Deserialize<SesNotification>(record.Sns.Message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
            });

            if (notification is null || !RelevantNotificationTypes.Contains(notification.NotificationType))            
            {
                return null;
            }

            if (notification.Mail is null)
            {
                context.Logger.LogLine("--> Mail property not found in notification message. Cannot process record.");
                return null;
            }

            var evt = new SesEvent
            {
                EventType = notification.NotificationType,
                Timestamp = notification.Mail.Timestamp,
                // MessageId = notification.Mail.
                Source = notification.Mail.Source,
                SourceArn = notification.Mail.SourceArn,
                SourceIp = notification.Mail.SourceIp,
                SendingAccountId = notification.Mail.SendingAccountId,
                FromAddress = notification.Mail.CommonHeaders.From.First(),
                Subject = notification.Mail.CommonHeaders.Subject,
                MessageDate = notification.Mail.CommonHeaders.Date,
                // Recipients = notification.Mail.CommonHeaders.To
            };

            if (notification.Mail.CommonHeaders.To?.Any() ?? false)
            {
                foreach(var toAddr in notification.Mail.CommonHeaders.To.Where(x => !string.IsNullOrEmpty(x)))
                {
                    if (toAddr.Contains(","))
                    {
                        evt.Recipients.AddRange(toAddr.Split(',', StringSplitOptions.RemoveEmptyEntries));
                    }
                    else
                    {
                        evt.Recipients.Add(toAddr);                        
                    }
                }
            }

            switch (notification.NotificationType)
            {
                case "Bounce" when notification.Bounce != null:
                    evt.SubType = $"{notification.Bounce.BounceType} - {notification.Bounce.BounceSubType}";
                    evt.FeedbackId = notification.Bounce.FeedbackId;
                    break;
            }

            return evt;
        }

        private async Task ProcessRecordAsync(SNSEvent.SNSRecord record, ILambdaContext context)
        {
            try
            { 
                var sesEvent = ParseRecord(record, context);
                if (sesEvent is not null)
                {
                    await SaveEventAsync(sesEvent);
                    context.Logger.LogLine($"Processed record for {sesEvent.EventType}");
                }
            }
            catch(Exception ex)
            {
                context.Logger.LogError($"Error: {ex.Message}");
            }

            await Task.CompletedTask;
        }

        private async Task SaveEventAsync(SesEvent evt)
        {
            await _dbContext.SaveAsync(evt);
        }
    }
}
