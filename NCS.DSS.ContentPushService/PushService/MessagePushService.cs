using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.ContentPushService.Auth;
using NCS.DSS.ContentPushService.Cosmos.Provider;
using NCS.DSS.ContentPushService.Listeners;
using NCS.DSS.ContentPushService.Models;
using Newtonsoft.Json;
using System.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace NCS.DSS.ContentPushService.PushService;

public class MessagePushService : IMessagePushService
{
    private readonly string _connectionString;
    private readonly ICosmosDBProvider _cosmosDbProvider;
    private readonly IOptions<ContentPushServiceConfigurationSettings> _configurationSettings;
    private readonly ILogger<MessagePushService> _logger;  
    private readonly int _serviceBusRetryAttemptLimit = 12;

    public MessagePushService(ICosmosDBProvider cosmosDbProvider,  IOptions<ContentPushServiceConfigurationSettings> configOptions, ILogger<MessagePushService> logger)
    {
        _cosmosDbProvider = cosmosDbProvider;
        _configurationSettings = configOptions;
        _logger = logger;
        _connectionString = configOptions.Value.ServiceBusConnectionString;
    }


    public async Task PushToTouchpoint(string touchpoint, ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions)
    {
        if (message == null)
        {
            _logger.LogError("Received Service Bus message is null");
            throw new ArgumentNullException(nameof(message));
        }

        if (string.IsNullOrEmpty(touchpoint))
        {
            _logger.LogError("Touchpoint is null");
            throw new ArgumentNullException(nameof(touchpoint));
        }

        var appIdUriAndClientUrl = GetAppIdUriAndClientUrl(_configurationSettings, _logger, touchpoint);
        var appIdUri = appIdUriAndClientUrl.Item1;
        var clientUrl = appIdUriAndClientUrl.Item2;

        var bearerToken = "";
        try
        {
            _logger.LogInformation("Attempting to get Access Token");
            bearerToken = await AuthenticationHelper.GetAccessToken(appIdUri, _logger, _configurationSettings);
        }
        catch (Exception ex)
        {
            _logger.LogError("Unable to get Access Token", ex);
            return;
        }
        _logger.LogInformation("Successfully retrieved Access Token");

        var client = HttpClientFactory.Create();
        var body = Encoding.UTF8.GetString(message.Body);
        _logger.LogInformation("Retrieved body from stream reader");

        _logger.LogInformation("Attempting to Deserialise customer MessageModel Body");
        var customer = JsonConvert.DeserializeObject<MessageModel>(body);

        if (customer == null)
        {
            _logger.LogWarning("Failed to Deserialise customer MessageModel Body. MessageModel Body is NULL");
            return;
        }
        _logger.LogInformation("Successfully Deserialised customer MessageModel Body");


        var notification = new Notification
        {
            CustomerId = customer.CustomerGuid.GetValueOrDefault(),
            ResourceURL = customer.URL,
            LastModifiedDate = customer.LastModifiedDate.GetValueOrDefault()
        };

        var notificationDC = new NotificationDC
        {
            TouchpointId = customer.TouchpointId,
            ResourceURL = customer.URL,
            LastModifiedDate = customer.LastModifiedDate.GetValueOrDefault()
        };

        var content = "";
        _logger.LogInformation("Attempting to Serialise notification to Json");
        if (customer.DataCollections.HasValue)
        {
            content = JsonConvert.SerializeObject(notificationDC);
            _logger.LogInformation("Successfully Serialised notificationDC object to Json");
        }
        else
        {
            content = JsonConvert.SerializeObject(notification);
            _logger.LogInformation("Attempting to Serialise notification object to Json");
        }

        var buffer = Encoding.UTF8.GetBytes(content);
        var byteContent = new ByteArrayContent(buffer);

        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        client.DefaultRequestHeaders.Add("OriginatingTouchpointId", customer.TouchpointId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        HttpResponseMessage response;

        try
        {
            _logger.LogInformation("Attempting to post notification to clientUrl {0}", clientUrl);
            response = await client.PostAsync(clientUrl, byteContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(string.Format("Error when attempting to post to clientUrl {0}", clientUrl), ex);
            throw;
        }

        if (response?.StatusCode == HttpStatusCode.OK || response?.StatusCode == HttpStatusCode.Created ||
            response?.StatusCode == HttpStatusCode.Accepted)
        {
            //Each messageReceiver must complete it's own message or we'll get a lockToken error
            _logger.LogInformation("Calling {FunctionName}", nameof(messageActions.CompleteMessageAsync));
            await messageActions.CompleteMessageAsync(message);
            _logger.LogInformation("Successfully called {FunctionName}", nameof(messageActions.CompleteMessageAsync));

            _logger.LogInformation(string.Format("Saving to DB: Responsecode: {0} ResourceUrl: {1} MessageId: {2}",
                (int)response?.StatusCode, notification.ResourceURL, message.MessageId));
            await SaveNotificationToDBAsync((int)response.StatusCode, message.MessageId, notification, appIdUri,
                clientUrl, bearerToken, true);
            _logger.LogInformation("Saved to DB successfully.");
        }
        else
        {
            _logger.LogWarning(string.Format("Response status code not 200, 201 or 202. Saving to DB. Responsecode: {0} ResourceUrl: {1} MessageId: {2}",
                (int)response?.StatusCode, notification.ResourceURL, message.MessageId));

            //Save notification to db
            await SaveNotificationToDBAsync((int)response.StatusCode, message.MessageId, notification, appIdUri,
                clientUrl, bearerToken, false);
            _logger.LogInformation("Successfully saved to DB.");

            //Create Servicebus resend client
            _logger.LogInformation("Attempting to Service Bus resend client");
            await using var serviceBusClient = new ServiceBusClient(_connectionString);
            _logger.LogInformation("Successfully created Service Bus resend client");
            var resendClient = serviceBusClient.CreateSender(touchpoint);


            //Get number of retries attempted
            message.ApplicationProperties.TryGetValue("RetryCount", out var rVal);
            var RetryCount = (int)rVal;

            if (RetryCount >= _serviceBusRetryAttemptLimit)
            {
                try
                {
                    //Deadletter as max retries exceeded
                    _logger.LogWarning("Message retry max attempts of {_serviceBusRetryAttemptLimit} reached. Sending message to dead letter queue.", _serviceBusRetryAttemptLimit.ToString());
                    await messageActions.DeadLetterMessageAsync(message, null, "MaxTriesExceeded",
                        $"Attempted to send notification to Endpoint {_serviceBusRetryAttemptLimit} times & failed!");
                    _logger.LogInformation("Successfully sent message to dead letter queue.");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error when attempting to deadletter!", ex);
                    throw;
                }
                finally
                {
                    await resendClient.CloseAsync();
                }
            }
            else
            {
                var resendMessage = new ServiceBusMessage(message.Body);

                _logger.LogInformation("Cloning message for retry attempt");

                //Schedule time for re-delivery attempt - UTC time + required number of seconds
                var retrySecs = GetRetrySeconds(RetryCount);
                resendMessage.ScheduledEnqueueTime = DateTime.UtcNow.AddSeconds(retrySecs);

                //Increment retry count by 1
                resendMessage.ApplicationProperties["RetryCount"] = RetryCount + 1;
                _logger.LogInformation(string.Format("Retrying message delivery Count {0}", RetryCount));

                try
                {
                    //Resend Message to the Topic
                    _logger.LogInformation("Attempting to resend message to the Topic");
                    await resendClient.SendMessageAsync(resendMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error when resending message!", ex);
                    throw;
                }
                finally
                {
                    await resendClient.CloseAsync();
                }

                //Complete original message
                await messageActions.CompleteMessageAsync(message);
                _logger.LogInformation("Message successfully resent");
            }
        }

        _logger.LogInformation("Exiting PushToTouchpoint");
    }

    public (string, string) GetAppIdUriAndClientUrl(IOptions<ContentPushServiceConfigurationSettings> configOptions, ILogger<MessagePushService> _logger, string touchpoint)
    {
        var config = configOptions.Value;

        switch (touchpoint)
        {
            case TouchPointListeners1.TP_0000000101:
                _logger.LogInformation("Retrieving App Id Uri and client Url for {touchpoint}", TouchPointListeners1.TP_0000000101);
                return (config.TouchPoint0000000101.AppIdUri, config.TouchPoint0000000101.Url);

            case TouchPointListeners1.TP_0000000102:
                _logger.LogInformation("Retrieving App Id Uri and client Url for {touchpoint}", TouchPointListeners1.TP_0000000102);
                return (config.TouchPoint0000000102.AppIdUri, config.TouchPoint0000000102.Url);

            case TouchPointListeners1.TP_0000000103:
                _logger.LogInformation("Retrieving App Id Uri and client Url for {touchpoint}", TouchPointListeners1.TP_0000000103);
                return (config.TouchPoint0000000103.AppIdUri, config.TouchPoint0000000103.Url);

            case TouchPointListeners1.TP_0000000104:
                _logger.LogInformation("Retrieving App Id Uri and client Url for {touchpoint}", TouchPointListeners1.TP_0000000104);
                return (config.TouchPoint0000000104.AppIdUri, config.TouchPoint0000000104.Url);

            case TouchPointListeners1.TP_0000000105:
                _logger.LogInformation("Retrieving App Id Uri and client Url for {touchpoint}", TouchPointListeners1.TP_0000000105);
                return (config.TouchPoint0000000105.AppIdUri, config.TouchPoint0000000105.Url);

            case TouchPointListeners1.TP_0000000106:
                _logger.LogInformation("Retrieving App Id Uri and client Url for {touchpoint}", TouchPointListeners1.TP_0000000106);
                return (config.TouchPoint0000000106.AppIdUri, config.TouchPoint0000000106.Url);

            case TouchPointListeners1.TP_0000000107:
                _logger.LogInformation("Retrieving App Id Uri and client Url for {touchpoint}", TouchPointListeners1.TP_0000000107);
                return (config.TouchPoint0000000107.AppIdUri, config.TouchPoint0000000107.Url);

            case TouchPointListeners1.TP_0000000108:
                _logger.LogInformation("Retrieving App Id Uri and client Url for {touchpoint}", TouchPointListeners1.TP_0000000108);
                return (config.TouchPoint0000000108.AppIdUri, config.TouchPoint0000000108.Url);

            case TouchPointListeners1.TP_0000000109:
                _logger.LogInformation("Retrieving App Id Uri and client Url for {touchpoint}", TouchPointListeners1.TP_0000000109);
                return (config.TouchPoint0000000109.AppIdUri, config.TouchPoint0000000109.Url);

            case TouchPointListeners2.TP_0000000201:
                _logger.LogInformation("Retrieving App Id Uri and client Url for {touchpoint}", TouchPointListeners2.TP_0000000201);
                return (config.TouchPoint0000000201.AppIdUri, config.TouchPoint0000000201.Url);

            case TouchPointListeners2.TP_0000000202:
                _logger.LogInformation("Retrieving App Id Uri and client Url for {touchpoint}", TouchPointListeners2.TP_0000000202);
                return (config.TouchPoint0000000202.AppIdUri, config.TouchPoint0000000202.Url);

            case TouchPointListeners2.TP_0000000203:
                _logger.LogInformation("Retrieving App Id Uri and client Url for {touchpoint}", TouchPointListeners2.TP_0000000203);
                return (config.TouchPoint0000000203.AppIdUri, config.TouchPoint0000000203.Url);

            case TouchPointListeners2.TP_0000000204:
                _logger.LogInformation("Retrieving App Id Uri and client Url for {touchpoint}", TouchPointListeners2.TP_0000000204);
                return (config.TouchPoint0000000204.AppIdUri, config.TouchPoint0000000204.Url);

            case TouchPointListeners2.TP_0000000205:
                _logger.LogInformation("Retrieving App Id Uri and client Url for {touchpoint}", TouchPointListeners2.TP_0000000205);
                return (config.TouchPoint0000000205.AppIdUri, config.TouchPoint0000000205.Url);

            case TouchPointListeners2.TP_0000000206:
                _logger.LogInformation("Retrieving App Id Uri and client Url for {touchpoint}", TouchPointListeners2.TP_0000000206);
                return (config.TouchPoint0000000206.AppIdUri, config.TouchPoint0000000206.Url);

            case TouchPointListeners2.TP_0000000207:
                _logger.LogInformation("Retrieving App Id Uri and client Url for {touchpoint}", TouchPointListeners2.TP_0000000207);
                return (config.TouchPoint0000000207.AppIdUri, config.TouchPoint0000000207.Url);

            case TouchPointListeners2.TP_0000000208:
                _logger.LogInformation("Retrieving App Id Uri and client Url for {touchpoint}", TouchPointListeners2.TP_0000000208);
                return (config.TouchPoint0000000208.AppIdUri, config.TouchPoint0000000208.Url);

            case TouchPointListeners2.TP_0000000209:
                _logger.LogInformation("Retrieving App Id Uri and client Url for {touchpoint}", TouchPointListeners2.TP_0000000209);
                return (config.TouchPoint0000000209.AppIdUri, config.TouchPoint0000000209.Url);

            case CareersHelplineListener.TP_0000000999:
                _logger.LogInformation("Retrieving App Id Uri and client Url for {touchpoint}", CareersHelplineListener.TP_0000000999);
                return (config.TouchPoint0000000999.AppIdUri, config.TouchPoint0000000999.Url);

            case TouchPointListenersTest.TP_9000000001:
                _logger.LogInformation("Retrieving App Id Uri and client Url for {touchpoint}", TouchPointListenersTest.TP_9000000001);
                return (config.TouchPoint9000000001.AppIdUri, config.TouchPoint9000000001.Url);

            case TouchPointListenersTest.TP_9000000002:
                _logger.LogInformation("Retrieving App Id Uri and client Url for {touchpoint}", TouchPointListenersTest.TP_9000000002);
                return (config.TouchPoint9000000002.AppIdUri, config.TouchPoint9000000002.Url);

            case TouchPointListenersTest.TP_9000000003:
                _logger.LogInformation("Retrieving App Id Uri and client Url for {touchpoint}", TouchPointListenersTest.TP_9000000003);
                return (config.TouchPoint9000000003.AppIdUri, config.TouchPoint9000000003.Url);

            default:
                _logger.LogError("Unrecognised touchpoint id: {touchpoint}", touchpoint);
                throw new InvalidDataException($"Unrecognised touchpoint id: {touchpoint}");
        }
    }

    public async Task SaveNotificationToDBAsync(int rspHttpCode, string MessageId, Notification rspNotification,
        string AppIdUri, string ClientUrl, string BearerToken, bool Success)
    {
        var DBNotification = new DBNotification
        {
            MessageId = MessageId,
            HttpCode = rspHttpCode,
            AppIdUri = AppIdUri,
            ClientUrl = ClientUrl,
            BearerToken = string.Empty,
            Success = Success,
            Notification = rspNotification,
            Timestamp = DateTime.UtcNow,
            id = Guid.NewGuid().ToString()
        };

        await _cosmosDbProvider.CreateNotificationAsync(DBNotification);
    }

    public int GetRetrySeconds(int RetryCount)
    {
        switch (RetryCount)
        {
            case 0:
                return 1;
            case 1:
                return 2;
            case 2:
                return 4;
            case 3:
                return 8;
            case 4:
                return 16;
            case 5:
                return 32;
            case 6:
                return 64;
            case 7:
                return 128;
            case 8:
                return 256;
            case 9:
                return 512;
            case 10:
                return 1024;
            case 11:
                return 2048;
            default:
                return 0;
        }
    }
}