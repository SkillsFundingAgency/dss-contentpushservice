using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.Models;
using NCS.DSS.ContentPushService.Utils;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace NCS.DSS.ContentPushService.Services
{
    public class DigitialIdentityService : IDigitialIdentityService
    {
        private readonly IRequeueService _requeueService;
        private readonly ILogger _logger;
        private readonly IDigitalIdentityClient _digitalidentityClient;

        public DigitialIdentityService(IRequeueService requeue, ILogger logger, IDigitalIdentityClient digitalidentityclient)
        {
            _requeueService = requeue;
            _logger = logger;
            _digitalidentityClient = digitalidentityclient;
        }

        public async Task SendMessage(string topic, string connectionString, Message message, ListenerSettings listenerSettings, IMessageReceiver messageReceiver)
        {
            if (message == null)
            {
                _logger.LogInformation("Digital Identity message is null, unable to post message");
                return;
            }

            var body = Encoding.UTF8.GetString(message.Body);
            var digitalidentity = JsonConvert.DeserializeObject<DigitalIdentity>(body);

            if (digitalidentity != null)
            {
                if (digitalidentity.DeleteDigitalIdentity == true)
                    await DeleteDigitalIdentity(topic, connectionString, message, listenerSettings, messageReceiver, digitalidentity);
                else if (digitalidentity.CreateDigitalIdentity == true)
                    await CreateNewDigitalIdentity(topic, connectionString, message, listenerSettings, messageReceiver, digitalidentity);
            }
            else
                _logger.LogInformation($"{message.MessageId} could not deserialize DigitalIdentity from message");

        }

        private async Task CreateNewDigitalIdentity(string topic, string connectionString, Message message, ListenerSettings listenerSettings, IMessageReceiver messageReceiver, DigitalIdentity digitalidentity)
        {
            //post digital identity to api
            var di = new DigitalIdentity() { FirstName = digitalidentity.FirstName, CustomerGuid = digitalidentity.CustomerGuid, LastName = digitalidentity.LastName, EmailAddress = digitalidentity.EmailAddress };
            var created = await _digitalidentityClient.Post(di, "endpoint");
            if (created)
            {
                await messageReceiver.CompleteAsync(GetLockToken(message));
                _logger.LogInformation($"{message.MessageId} has been successfully delivered to Digital Identity Api, message removed from topic");
            }
            else
            {
                //requeue message on topic if message was not successfully posted
                var retry = await _requeueService.RequeueItem(topic, connectionString, 12, message);
                if (retry)
                {
                    await messageReceiver.DeadLetterAsync(GetLockToken(message), "MaxTriesExceeded", "Attempted to send notification to Endpoint 12 times & failed!");
                    _logger.LogInformation($"{message.MessageId} has been deadlettered after 12 attempts");
                }
            }
        }

        private async Task DeleteDigitalIdentity(string topic, string connectionString, Message message, ListenerSettings listenerSettings, IMessageReceiver messageReceiver, DigitalIdentity digitalidentity)
        {
            //delete digital identity
            var di = new DigitalIdentity() { FirstName = digitalidentity.FirstName, CustomerGuid = digitalidentity.CustomerGuid, LastName = digitalidentity.LastName, EmailAddress = digitalidentity.EmailAddress };
            var delete = await _digitalidentityClient.Delete(new { }, $"Delete?{digitalidentity.CustomerGuid}");
            if (delete)
            {
                await messageReceiver.CompleteAsync(GetLockToken(message));
                _logger.LogInformation($"{message.MessageId} has been successfully deleted Digital account");
            }
            else
            {
                //requeue message on topic if message was not successfully posted
                var retry = await _requeueService.RequeueItem(topic, connectionString, 12, message);
                if (retry)
                {
                    await messageReceiver.DeadLetterAsync(GetLockToken(message), "MaxTriesExceeded", "Attempted to send notification to Endpoint 12 times & failed!");
                    _logger.LogInformation($"{message.MessageId} has been deadlettered after 12 attempts");
                }
            }
        }

        private string GetLockToken(Message msg)
        {
            // msg.SystemProperties.LockToken Get property throws exception if not set. Return null instead.
            return msg.SystemProperties.IsLockTokenSet ? msg.SystemProperties.LockToken : null;
        }
    }
}
