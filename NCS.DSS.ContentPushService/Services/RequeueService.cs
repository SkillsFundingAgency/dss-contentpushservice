using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NCS.DSS.ContentPushService.Services
{
    public class RequeueService : IRequeueService
    {
        private readonly ILogger<RequeueService> _logger;
        public RequeueService(ILogger<RequeueService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> RequeueItem(string topicName, string connectionString, int maxRetryCount, Message message)
        {
            var resendClient = new TopicClient(connectionString, topicName);
            var resendMessage = message.Clone();
            message.UserProperties.TryGetValue("RetryCount", out object rVal);
            var retryCount = (int)rVal;


            //Schedule time for re-delivery attempt - UTC time + required number of seconds
            if (retryCount < maxRetryCount)
            {
                int retrySecs = GetRetrySeconds(retryCount);
                resendMessage.ScheduledEnqueueTimeUtc = DateTime.UtcNow.AddSeconds(retrySecs);
                resendMessage.UserProperties["RetryCount"] = retryCount + 1;
                
                _logger.LogInformation("Attempting to resend message to the Topic");
                await resendClient.SendAsync(resendMessage);
                return true;
            }
            return false;
        }

        public int GetRetrySeconds(int RetryCount)
        {
            switch (RetryCount)
            {
                case (0):
                    return 1;
                case (1):
                    return 2;
                case (2):
                    return 4;
                case (3):
                    return 8;
                case (4):
                    return 16;
                case (5):
                    return 32;
                case (6):
                    return 64;
                case (7):
                    return 128;
                case (8):
                    return 256;
                case (9):
                    return 512;
                case (10):
                    return 1024;
                case (11):
                    return 2048;
                default:
                    return 0;
            }
        }
    }
}
