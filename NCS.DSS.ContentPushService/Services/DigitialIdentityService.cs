using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.Constants;
using NCS.DSS.ContentPushService.Models;
using NCS.DSS.ContentPushService.Utils;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace NCS.DSS.ContentPushService.Services
{
    public class DigitialIdentityService : IDigitialIdentityService
    {
        private readonly IRequeueService _requeueService;
        private readonly IDigitalIdentityClient _digitalidentityClient;

        public DigitialIdentityService(IRequeueService requeue, IDigitalIdentityClient digitalidentityclient)
        {
            _requeueService = requeue;
            _digitalidentityClient = digitalidentityclient;
        }

        public async Task<DigitalIdentityServiceActions> SendMessage(string topic, Message message, IMessageReceiverService messageReceiver, ILogger logger)
        {

            if (message == null)
            {
                logger.LogInformation("Digital Identity message is null, unable to post message");
                return DigitalIdentityServiceActions.CouldNotAction;
            }

            var body = Encoding.UTF8.GetString(message.Body);
            var digitalidentity = JsonConvert.DeserializeObject<DigitalIdentity>(body);
            var successfullyActioned = false;
            var token = GetLockToken(message);

            if (digitalidentity != null)
            {
                if (digitalidentity.DeleteDigitalIdentity == true)
                {
                    successfullyActioned = await DeleteDigitalIdentity(digitalidentity);
                }
                else if (digitalidentity.CreateDigitalIdentity == true)
                {
                    successfullyActioned = await CreateNewDigitalIdentity(digitalidentity);
                }
                else if (digitalidentity.ChangeEmailAddress == true)
                {
                    successfullyActioned = await ChangeEmail(digitalidentity);
                }
                else if (digitalidentity.UpdateDigitalIdentity == true)
                {
                    successfullyActioned = await UpdateUser(digitalidentity);
                }
                else
                {
                    //cannot action digital identity, deadletter message in queue.
                    var errMsg = $"Unable to determine if digital identity needs to be updated/created/deleted for customer: {digitalidentity.CustomerGuid}";
                    await messageReceiver.DeadLetterAsync(token, "MaxTriesExceeded", errMsg);
                    throw new Exception(errMsg);
                }

                //if actioned message successfully, then remove message from queue
                if (successfullyActioned)
                {
                    logger.LogInformation($"Successfully actioned CustomerId:{digitalidentity.CustomerGuid} - Create: { digitalidentity.CreateDigitalIdentity}, Delete:{digitalidentity.DeleteDigitalIdentity}, ChangeEmail: {digitalidentity.ChangeEmailAddress}");
                    await messageReceiver.CompleteAsync(token);
                    return DigitalIdentityServiceActions.SuccessfullyActioned;
                }
                else
                {
                    //requeue message on topic if message was not successfully actioned
                    var retry = await _requeueService.RequeueItem(topic, 12, message, logger);
                    if (!retry)
                    {
                        await messageReceiver.DeadLetterAsync(token, "MaxTriesExceeded", "Attempted to send notification to Endpoint 12 times & failed!");
                        logger.LogInformation($"CustomerId:{digitalidentity.CustomerGuid} - message:{message.MessageId} has been deadlettered after 12 attempts");
                        return DigitalIdentityServiceActions.DeadLettered;
                    }
                    return DigitalIdentityServiceActions.Requeued;
                }
            }
            else
            {
                logger.LogInformation($"{message.MessageId} could not deserialize DigitalIdentity from message");
                return DigitalIdentityServiceActions.CouldNotAction;
            }
        }

        private async Task<bool> ChangeEmail(DigitalIdentity digitalidentity)
        {
            var di = new { ObjectID = digitalidentity.IdentityStoreId, digitalidentity.NewEmail, digitalidentity.CurrentEmail };
            return await _digitalidentityClient.Post(di, "ChangeEmail");
        }

        private async Task<bool> UpdateUser(DigitalIdentity digitalidentity)
        {
            var di = new { ObjectId = digitalidentity.IdentityStoreId, FirstName = digitalidentity.FirstName, LastName = digitalidentity.LastName };
            return await _digitalidentityClient.Put(di, "UpdateUser");
        }

        private async Task<bool> CreateNewDigitalIdentity(DigitalIdentity digitalidentity)
        {
            var di = new { givenName = digitalidentity.FirstName, lastName = digitalidentity.LastName, email = digitalidentity.EmailAddress, CustomerId = digitalidentity.CustomerGuid };
            return await _digitalidentityClient.Post(di, "SignUpInvitation");
        }

        private async Task<bool> DeleteDigitalIdentity(DigitalIdentity digitalidentity)
        {
            return await _digitalidentityClient.Delete(new { }, $"DeleteUser?id={digitalidentity.IdentityStoreId}");
        }

        private string GetLockToken(Message msg)
        {
            return (msg.SystemProperties?.IsLockTokenSet == true) ? msg.SystemProperties?.LockToken : null;
        }
    }
}
