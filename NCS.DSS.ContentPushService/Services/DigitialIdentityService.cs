using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.Models;
using NCS.DSS.ContentPushService.Utils;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using System;
using Microsoft.Azure.Documents.Client;

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

        public async Task SendMessage(string topic, string connectionString, Message message, ListenerSettings listenerSettings, IMessageReceiver messageReceiver, ILogger logger)
        {
            
            if (message == null)
            {
                logger.LogInformation("Digital Identity message is null, unable to post message");
                return;
            }

            var body = Encoding.UTF8.GetString(message.Body);
            var digitalidentity = JsonConvert.DeserializeObject<DigitalIdentity>(body);
            var successfullyActioned = false;

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
                else
                {
                    //cannot action digital identity, deadletter message in queue.
                    var errMsg = $"Unable to determine if digital identity needs to be updated/created/deleted for customer: {digitalidentity.CustomerGuid}";
                    await messageReceiver.DeadLetterAsync(GetLockToken(message), "MaxTriesExceeded", errMsg);
                    throw new Exception(errMsg);
                }

                //if actioned message successfully, then remove message from queue
                if(successfullyActioned)
                {
                    await messageReceiver.CompleteAsync(GetLockToken(message));
                }
                else
                {
                    //requeue message on topic if message was not successfully actioned
                    var retry = await _requeueService.RequeueItem(topic, connectionString, 12, message);
                    if (!retry)
                    {
                        await messageReceiver.DeadLetterAsync(GetLockToken(message), "MaxTriesExceeded", "Attempted to send notification to Endpoint 12 times & failed!");
                        logger.LogInformation($"{message.MessageId} has been deadlettered after 12 attempts");
                    }
                }
            }
            else
                logger.LogInformation($"{message.MessageId} could not deserialize DigitalIdentity from message");

        }

        private async Task<bool> ChangeEmail(DigitalIdentity digitalidentity)
        {
            var di = new { digitalidentity.NewEmail, digitalidentity.CurrentEmail };
            return await _digitalidentityClient.Post(di, "ChangeEmail");
        }

        private async Task<bool> CreateNewDigitalIdentity(DigitalIdentity digitalidentity)
        {
            var day = digitalidentity.DoB?.Day;
            var month = digitalidentity.DoB?.Month;
            var year = digitalidentity.DoB?.Year;
            var di = new { GivenName = digitalidentity.FirstName, Surname = digitalidentity.LastName, Email = digitalidentity.EmailAddress, Day = day, Month = month, Year = year, CustomerId = digitalidentity.CustomerGuid, IsAided = false, ObjectId = Guid.NewGuid() };
            return await _digitalidentityClient.Post(di, "NCSDSSUserCreation");
        }

        private async Task<bool> DeleteDigitalIdentity(DigitalIdentity digitalidentity)
        {
            return await _digitalidentityClient.Delete(new { }, $"Delete?id={digitalidentity.IdentityStoreId}");
        }

        private string GetLockToken(Message msg)
        {
            // msg.SystemProperties.LockToken Get property throws exception if not set. Return null instead.
            return msg.SystemProperties.IsLockTokenSet ? msg.SystemProperties.LockToken : null;
        }
    }
}
