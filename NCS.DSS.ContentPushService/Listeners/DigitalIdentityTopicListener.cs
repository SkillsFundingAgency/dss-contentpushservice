using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.Models;
using NCS.DSS.ContentPushService.Services;

namespace NCS.DSS.ContentPushService.Listeners
{
    public class DigitalIdentityTopicListener
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string TopicName = "digitalidentity";
        private const string SubscriptionName = "DigitalIdentity";
        private const string AppIdUri = "gg";
        private const string ClientUrl = "gg";
        private const string FunctionName = "DigitalIdentityTopicListener";
        private const string ServiceBusConnectionString = "ServiceBusConnectionString";
        private readonly IDigitialIdentityService _digitalidentity;

        public DigitalIdentityTopicListener(IDigitialIdentityService digitalidentity)
        {
            _digitalidentity = digitalidentity;
        }

        [FunctionName(FunctionName)]
        public async Task RunAsync([ServiceBusTrigger(TopicName, SubscriptionName, Connection = ServiceBusConnectionString)] Message serviceBusMessage, MessageReceiver messageReceiver, ILogger log)
        {
            var listinerSettings = new ListenerSettings
            {
                AppIdUri = AppIdUri,
                ClientUrl = ClientUrl,
                SubscriptionName = SubscriptionName,
                TopicName = TopicName
            };
           
            var connectionStr = Environment.GetEnvironmentVariable(ServiceBusConnectionString);
            
            log.LogInformation("DigitalIdentityTopicListener received received message");
            await _digitalidentity.SendMessage(TopicName, connectionStr,  serviceBusMessage, listinerSettings, messageReceiver);
        }
    }
}
