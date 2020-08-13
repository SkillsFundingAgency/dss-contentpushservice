using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using System;
using System.Threading.Tasks;

namespace NCS.DSS.ContentPushService.Services
{
    public class MessageReceiverService : IMessageReceiverService
    {
        private const string ServiceBusConnectionString = "ServiceBusConnectionString";
        private const string topic = "digitalidentity";
        private const string subscription = "Digitalidentity";

        public async Task CompleteAsync(string msg)
        {
            var receiver = GetMessageReceiver(topic, subscription);
            await receiver.CompleteAsync(msg);
        }

        public async Task DeadLetterAsync(string msg, string error, string errorDesc)
        {
            var receiver = GetMessageReceiver(topic, subscription);
            await receiver.DeadLetterAsync(msg);
        }

        public MessageReceiver GetMessageReceiver(string topic, string subscription)
        {
            var connectionStr = Environment.GetEnvironmentVariable(ServiceBusConnectionString);
            string path = EntityNameHelper.FormatSubscriptionPath(topic, subscription);
            return new MessageReceiver(connectionStr, path);
        }
    }

    public interface IMessageReceiverService
    {
        Task CompleteAsync(string msg);
        Task DeadLetterAsync(string msg, string error, string errorDesc);
    }
}
