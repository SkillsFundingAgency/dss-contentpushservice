using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using NCS.DSS.ContentPushService.Constants;

namespace NCS.DSS.ContentPushService.Services;

public interface IDigitialIdentityService
{
    Task<DigitalIdentityServiceActions> SendMessage(string topic, ServiceBusReceivedMessage serviceBusMessage,
        ServiceBusMessageActions messageActions);
}