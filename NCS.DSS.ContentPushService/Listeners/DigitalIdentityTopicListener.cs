using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
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
    private readonly ILogger<DigitalIdentityTopicListener> _logger;

    public DigitalIdentityTopicListener(IDigitialIdentityService digitalidentity, ILogger<DigitalIdentityTopicListener> logger)
    {
        _digitalidentity = digitalidentity;
        _logger = logger;
    }

    [Function(FunctionName)]
    public async Task RunAsync(
        [ServiceBusTrigger(TopicName, SubscriptionName, Connection = ServiceBusConnectionString)]
        ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions)
    {
        _logger.LogInformation("DigitalIdentityTopicListener is attempting send message to {TopicName}", TopicName);
        await _digitalidentity.SendMessage(TopicName, serviceBusMessage, messageActions);
    }
}