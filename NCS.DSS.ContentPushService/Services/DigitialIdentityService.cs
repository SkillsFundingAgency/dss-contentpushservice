using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.Constants;
using NCS.DSS.ContentPushService.Models;
using NCS.DSS.ContentPushService.Utils;
using Newtonsoft.Json;
using System.Text;

namespace NCS.DSS.ContentPushService.Services
{
    public class DigitialIdentityService : IDigitialIdentityService
    {
        private readonly IRequeueService _requeueService;
        private readonly IDigitalIdentityClient _digitalidentityClient;
        private readonly ILogger<DigitialIdentityService> _logger;
        private readonly int _serviceBusRetryAttemptLimit = 12;

        public DigitialIdentityService(IRequeueService requeue, IDigitalIdentityClient digitalidentityclient, ILogger<DigitialIdentityService> logger)
        {
            _requeueService = requeue;
            _digitalidentityClient = digitalidentityclient;
            _logger = logger;
        }

        public async Task<DigitalIdentityServiceActions> SendMessage(string topic, ServiceBusReceivedMessage serviceBusMessage, ServiceBusMessageActions messageActions)
        {

            if (serviceBusMessage == null)
            {
                _logger.LogWarning("Digital Identity message is null, unable to post message");
                return DigitalIdentityServiceActions.CouldNotAction;
            }

            var body = Encoding.UTF8.GetString(serviceBusMessage.Body);
            _logger.LogInformation("Deserializing DigitalIdentity body");
            var digitalidentity = JsonConvert.DeserializeObject<DigitalIdentity>(body);
            var successfullyActioned = false;

            if (digitalidentity != null)
            {
                _logger.LogInformation("Successfully deserialized DigitalIdentity body");

                if (digitalidentity.DeleteDigitalIdentity == true)
                {
                    _logger.LogInformation("Attempting to DigitalIdentity. Calling function: {FunctionName}", nameof(DeleteDigitalIdentity));
                    successfullyActioned = await DeleteDigitalIdentity(digitalidentity);
                }
                else if (digitalidentity.CreateDigitalIdentity == true)
                {
                    _logger.LogInformation("Attempting to create new DigitalIdentity. Calling function: {FunctionName}", nameof(CreateNewDigitalIdentity));
                    successfullyActioned = await CreateNewDigitalIdentity(digitalidentity);
                }
                else if (digitalidentity.ChangeEmailAddress == true)
                {
                    _logger.LogInformation("Attempting to change DigitalIdentity email address. Calling function: {FunctionName}", nameof(ChangeEmail));
                    successfullyActioned = await ChangeEmail(digitalidentity);
                }
                else if (digitalidentity.UpdateDigitalIdentity == true)
                {
                    _logger.LogInformation("Attempting to update DigitalIdentity. Calling function: {FunctionName}", nameof(UpdateUser));
                    successfullyActioned = await UpdateUser(digitalidentity);
                }
                else
                {
                    //cannot action digital identity, deadletter message in queue.
                    var errMsg = $"Unable to determine if digital identity needs to be updated/created/deleted for customer: {digitalidentity.CustomerGuid}";
                    _logger.LogError(errMsg);
                    _logger.LogInformation("Attempting to send message to dead letter queue.");
                    await messageActions.DeadLetterMessageAsync(serviceBusMessage, null, "MaxTriesExceeded",
                        errMsg);
                    throw new Exception(errMsg);
                }

                //if actioned message successfully, then remove message from queue
                if (successfullyActioned)
                {
                    _logger.LogInformation($"Successfully actioned CustomerId:{digitalidentity.CustomerGuid} - Create: {digitalidentity.CreateDigitalIdentity}, Delete:{digitalidentity.DeleteDigitalIdentity}, ChangeEmail: {digitalidentity.ChangeEmailAddress}");
                    await messageActions.CompleteMessageAsync(serviceBusMessage);
                    return DigitalIdentityServiceActions.SuccessfullyActioned;
                }
                else
                {
                    _logger.LogInformation($"Failed to action CustomerId:{digitalidentity.CustomerGuid} - Create: {digitalidentity.CreateDigitalIdentity}, Delete:{digitalidentity.DeleteDigitalIdentity}, ChangeEmail: {digitalidentity.ChangeEmailAddress}");

                    //requeue message on topic if message was not successfully actioned
                    var retry = await _requeueService.RequeueItem(topic, _serviceBusRetryAttemptLimit, serviceBusMessage);
                    if (!retry)
                    {
                        await messageActions.DeadLetterMessageAsync(serviceBusMessage, null, "MaxTriesExceeded",
                            "Attempted to send notification to Endpoint " + _serviceBusRetryAttemptLimit.ToString() + " times & failed!");
                        _logger.LogInformation($"CustomerId:{digitalidentity.CustomerGuid} - message:{serviceBusMessage.MessageId} has been deadlettered after {_serviceBusRetryAttemptLimit} attempts");
                        return DigitalIdentityServiceActions.DeadLettered;
                    }
                    else
                    {
                        _logger.LogInformation("Successfully resent notification to Endpoint.");
                        await messageActions.CompleteMessageAsync(serviceBusMessage);
                    }

                    return DigitalIdentityServiceActions.Requeued;
                }
            }
            else
            {
                _logger.LogWarning($"{serviceBusMessage.MessageId} could not deserialize DigitalIdentity from message");
                return DigitalIdentityServiceActions.CouldNotAction;
            }
        }

        private async Task<bool> ChangeEmail(DigitalIdentity digitalidentity)
        {
            var di = new { ObjectID = digitalidentity.IdentityStoreId, digitalidentity.NewEmail, digitalidentity.CurrentEmail };
            var jsonobj = JsonConvert.SerializeObject(di);
            return await _digitalidentityClient.Post(digitalidentity.CustomerGuid?.ToString(), digitalidentity.IdentityStoreId?.ToString(), jsonobj, "ChangeEmail");
        }

        private async Task<bool> UpdateUser(DigitalIdentity digitalidentity)
        {
            var di = new { ObjectId = digitalidentity.IdentityStoreId, FirstName = digitalidentity.FirstName, LastName = digitalidentity.LastName };
            var jsonobj = JsonConvert.SerializeObject(di);
            return await _digitalidentityClient.Put(digitalidentity.CustomerGuid?.ToString(), digitalidentity.IdentityStoreId?.ToString(), jsonobj, "UpdateUser");
        }

        private async Task<bool> CreateNewDigitalIdentity(DigitalIdentity digitalidentity)
        {
            var di = new { givenName = digitalidentity.FirstName, lastName = digitalidentity.LastName, email = digitalidentity.EmailAddress, CustomerId = digitalidentity.CustomerGuid };
            var jsonobj = JsonConvert.SerializeObject(di);
            return await _digitalidentityClient.Post(digitalidentity.CustomerGuid?.ToString(), digitalidentity.IdentityStoreId?.ToString(), jsonobj, "SignUpInvitation");
        }

        private async Task<bool> DeleteDigitalIdentity(DigitalIdentity digitalidentity)
        {
            return await _digitalidentityClient.Delete(digitalidentity.CustomerGuid?.ToString(), digitalidentity.IdentityStoreId?.ToString(), "", $"DeleteUser?id={digitalidentity.IdentityStoreId}");
        }
    }
}
