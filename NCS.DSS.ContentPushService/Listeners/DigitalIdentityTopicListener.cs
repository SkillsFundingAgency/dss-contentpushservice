using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.Services;

namespace NCS.DSS.ContentPushService.Listeners;

public class DigitalIdentityTopicListener
{
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
    public async Task RunAsync(
        [ServiceBusTrigger(TopicName, SubscriptionName, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions, ILogger log)
    {
        log.LogInformation("DigitalIdentityTopicListener received received message");
        await _digitalidentity.SendMessage(TopicName, serviceBusMessage, messageActions);
    }
}