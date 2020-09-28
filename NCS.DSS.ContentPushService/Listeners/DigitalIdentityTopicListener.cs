using System;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.Constants;
using NCS.DSS.ContentPushService.Models;
using NCS.DSS.ContentPushService.Services;

namespace NCS.DSS.ContentPushService.Listeners
{
    public class DigitalIdentityTopicListener
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string TopicName = "digitalidentity";
        private const string SubscriptionName = "DigitalIdentity";
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
            var service = new MessageReceiverService(messageReceiver);
            log.LogInformation("DigitalIdentityTopicListener received received message");
            await _digitalidentity.SendMessage(TopicName,  serviceBusMessage, service, log);
        }
    }
}
