using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using NCS.DSS.ContentPushService.Auth;
using NCS.DSS.ContentPushService.Cosmos.Provider;
using NCS.DSS.ContentPushService.Models;
using Newtonsoft.Json;

namespace NCS.DSS.ContentPushService.PushService;

public class MessagePushService : IMessagePushService
{
    private readonly string _connectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");


    public async Task PushToTouchpoint(string touchpoint, ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions, ILogger log)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (string.IsNullOrEmpty(touchpoint)) throw new ArgumentNullException(nameof(touchpoint));

        var appIdUri = GetEnvironmentVariable($"touchpoint.{touchpoint}.AppIdUri", log);
        var clientUrl = GetEnvironmentVariable($"touchpoint.{touchpoint}.Url", log);

        var bearerToken = "";
        try
        {
            bearerToken = await AuthenticationHelper.GetAccessToken(appIdUri);
        }
        catch (Exception)
        {
            log.LogWarning("Unable to get Access Token");
        }


        var client = HttpClientFactory.Create();
        var body = Encoding.UTF8.GetString(message.Body);

        log.LogInformation("got body from stream reader");

        var customer = JsonConvert.DeserializeObject<MessageModel>(body);

        log.LogInformation("Deserialised MessageModel Body");

        if (customer == null) return;

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
        if (customer.DataCollections.HasValue)
            content = JsonConvert.SerializeObject(notificationDC);
        else
            content = JsonConvert.SerializeObject(notification);

        var buffer = Encoding.UTF8.GetBytes(content);
        var byteContent = new ByteArrayContent(buffer);

        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        client.DefaultRequestHeaders.Add("OriginatingTouchpointId", customer.TouchpointId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        HttpResponseMessage response;

        try
        {
            response = await client.PostAsync(clientUrl, byteContent);
        }
        catch (Exception)
        {
            log.LogError(string.Format("Error when attempting to post to clientUrl {0}", clientUrl));
            throw;
        }

        if (response?.StatusCode == HttpStatusCode.OK || response?.StatusCode == HttpStatusCode.Created ||
            response?.StatusCode == HttpStatusCode.Accepted)
        {
            //Each messageReceiver must complete it's own message or we'll get a lockToken error
            await messageActions.CompleteMessageAsync(message);

            await SaveNotificationToDBAsync((int)response.StatusCode, message.MessageId, notification, appIdUri,
                clientUrl, bearerToken, true);
            log.LogInformation(string.Format("Saving to DB: Responsecode: {0} ResourceUrl: {1} MessageId: {2}",
                (int)response?.StatusCode, notification.ResourceURL, message.MessageId));
        }
        else
        {
            //Save notification to db
            await SaveNotificationToDBAsync((int)response.StatusCode, message.MessageId, notification, appIdUri,
                clientUrl, bearerToken, false);
            log.LogInformation(string.Format("Saving to DB: Responsecode: {0} ResourceUrl: {1} MessageId: {2}",
                (int)response?.StatusCode, notification.ResourceURL, message.MessageId));

            //Create Servicebus resend client

            await using var serviceBusClient = new ServiceBusClient(_connectionString);
            var resendClient = serviceBusClient.CreateSender(touchpoint);


            //Get number of retries attempted
            message.ApplicationProperties.TryGetValue("RetryCount", out var rVal);
            var RetryCount = (int)rVal;

            if (RetryCount >= 12)
            {
                try
                {
                    //Deadletter as max retries exceeded
                    await messageActions.DeadLetterMessageAsync(message, "MaxTriesExceeded",
                        "Attempted to send notification to Endpoint 12 times & failed!");
                    //await message.DeadLetterAsync("MaxTriesExceeded", "Attempted to send notification to Endpoint 12 times & failed!");
                    log.LogInformation("Message retry max attempts reached");
                }
                catch (Exception)
                {
                    log.LogError("Error when attempting to deadletter!");
                    throw;
                }
                finally
                {
                    //resendClient.Close();
                    await resendClient.CloseAsync();
                }
            }
            else
            {
                var resendMessage = new ServiceBusMessage(message.Body);

                log.LogInformation("Cloning message for retry attempt");

                //Schedule time for re-delivery attempt - UTC time + required number of seconds
                var retrySecs = GetRetrySeconds(RetryCount);
                resendMessage.ScheduledEnqueueTime = DateTime.UtcNow.AddSeconds(retrySecs);

                //Increment retry count by 1
                resendMessage.ApplicationProperties["RetryCount"] = RetryCount + 1;
                log.LogInformation(string.Format("Retrying message delivery Count {0}", RetryCount));

                try
                {
                    //Resend Message to the Topic
                    log.LogInformation("Attempting to resend message to the Topic");
                    await resendClient.SendMessageAsync(resendMessage);
                }
                catch (Exception)
                {
                    log.LogError("Error when resending message!");
                    throw;
                }
                finally
                {
                    //resendClient.Close();
                    await resendClient.CloseAsync();
                }

                //Complete original message
                await messageActions.CompleteMessageAsync(message);
                log.LogInformation("Message successfully resent");
            }
        }

        log.LogInformation("Exiting PushToTouchpoint");
    }


    private string GetEnvironmentVariable(string path, ILogger log)
    {
        log.LogInformation("Attempting to read environment variable {0}", path);
        var value = Environment.GetEnvironmentVariable(path);
        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(path));

        log.LogInformation("Value returned {0}", value);
        return value;
    }

    public static int GetRetrySeconds(int RetryCount)
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

    public static async Task SaveNotificationToDBAsync(int rspHttpCode, string MessageId, Notification rspNotification,
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
            Timestamp = DateTime.UtcNow
        };

        var documentDbProvider = new DocumentDBProvider();

        await documentDbProvider.CreateNotificationAsync(DBNotification);
    }
}