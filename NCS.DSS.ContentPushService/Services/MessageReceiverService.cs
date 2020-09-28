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
        private MessageReceiver _messageReciever;

        public MessageReceiverService(MessageReceiver receiver)
        {
            _messageReciever = receiver;
        }

        public async Task CompleteAsync(string lockToken)
        {
            if(_messageReciever == null)
                _messageReciever = GetMessageReceiver(topic, subscription);
            await _messageReciever.CompleteAsync(lockToken);
        }

        public async Task DeadLetterAsync(string lockToken, string error, string errorDesc)
        {
            if (_messageReciever == null)
                _messageReciever = GetMessageReceiver(topic, subscription);
            await _messageReciever.DeadLetterAsync(lockToken);
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
        Task CompleteAsync(string lockToken);
        Task DeadLetterAsync(string lockToken, string error, string errorDesc);
    }
}
