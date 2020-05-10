using System.Collections.Generic;
using System.Linq;

using Xunit;
using Amazon.Lambda.SNSEvents;
using System.IO;
using System;
using Amazon.Lambda.TestUtilities;
using System.Threading.Tasks;

namespace SesCollector.Collector.Tests
{
    public class FunctionTest
    {
        [Fact]
        public void ParseRecord_BounceNotificationWithDsn_ParsesCorrectly()
        {
            var logger = new TestLambdaLogger();
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var sesNotificationMessage = File.ReadAllText(@"TestMessages\BounceWithDsn.json");
            var snsEvent = new SNSEvent
            {
                Records = new List<SNSEvent.SNSRecord>
                {
                    new SNSEvent.SNSRecord
                    {
                        Sns = new SNSEvent.SNSMessage
                        {
                            Message = sesNotificationMessage
                        }
                    }
                }
            };

            var function = new Function();
            var sesEvent = function.ParseRecord(snsEvent.Records.First(), context);

            Assert.NotNull(sesEvent);
            Assert.Equal("Bounce", sesEvent.EventType);
            Assert.Equal("Permanent - General", sesEvent.SubType);
            Assert.NotNull(sesEvent.FeedbackId);

            // 2016-01-27T14:59:38.237Z
            Assert.Equal(new DateTime(2016, 01, 27, 14, 59, 38, 237, DateTimeKind.Utc), sesEvent.Timestamp);
            Assert.Equal("john@example.com", sesEvent.Source);
            Assert.NotNull(sesEvent.SourceArn);
            Assert.Equal("127.0.3.0", sesEvent.SourceIp);
            Assert.Equal("123456789012", sesEvent.SendingAccountId);

            Assert.Equal("John Doe <john@example.com>", sesEvent.FromAddress);
            Assert.Equal("Hello", sesEvent.Subject);
            Assert.NotNull(sesEvent.MessageDate);
            Assert.NotEmpty(sesEvent.Recipients);
            Assert.Contains("Jane Doe <jane@example.com>", sesEvent.Recipients.First());
        }

        [Fact]
        public async Task TestSQSEventLambdaFunction()
        {
            var sesNotificationMessage = File.ReadAllText(@"TestMessages\BounceWithDsn.json");
            var snsEvent = new SNSEvent
            {
                Records = new List<SNSEvent.SNSRecord>
                {
                    new SNSEvent.SNSRecord
                    {
                        Sns = new SNSEvent.SNSMessage()
                        {
                            Message = sesNotificationMessage
                        }
                    }
                }
            };

            var logger = new TestLambdaLogger();
            var context = new TestLambdaContext
            {
                Logger = logger
            };

            var function = new Function();
            await function.FunctionHandler(snsEvent, context);

            Assert.Contains("Processed record for Bounce", logger.Buffer.ToString());
        }
    }
}
