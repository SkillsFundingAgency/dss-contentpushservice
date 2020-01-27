using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using NCS.DSS.ContentPushService.Auth;
using NCS.DSS.ContentPushService.Cosmos.Provider;
using NCS.DSS.ContentPushService.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.ServiceBus;
using System.Threading;
using Microsoft.Azure.ServiceBus.Core;

namespace NCS.DSS.ContentPushService.PushService
{
    public class MessagePushService : IMessagePushService
    {
        readonly string _connectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");

        public async Task PushToTouchpoint(string AppIdUri, string ClientUrl, Message message, string TopicName, 
            MessageReceiver messageReceiver, ILogger log)
        {
            log.LogInformation("Entering PushToTouchpoint:-   Attempting to push to touchpoint appIdUri: " + AppIdUri);

            if (message == null)
            {
                return;
            }

            string appIdUri = Environment.GetEnvironmentVariable(AppIdUri).ToString();
            if ((appIdUri == null) || (AppIdUri == ""))
                throw new Exception("AppIdUri: " + AppIdUri + " does not exist!");

            var bearerToken = await AuthenticationHelper.GetAccessToken(appIdUri);

            string clientUrl = Environment.GetEnvironmentVariable(ClientUrl).ToString();
            if ((clientUrl == null) || (ClientUrl == ""))
                throw new Exception("ClientUrl: " + clientUrl + " does not exist!");

            var client = HttpClientFactory.Create();
            var body = Encoding.UTF8.GetString(message.Body);

            log.LogInformation("got body from stream reader");

            var customer = JsonConvert.DeserializeObject<MessageModel>(body);

            log.LogInformation("Deserialised MessageModel Body");

            if (customer == null)
            {
                return;
            }

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
            {
                content = JsonConvert.SerializeObject(notificationDC);
            }
            else
            {
                content = JsonConvert.SerializeObject(notification);
            }

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

            //For testing purposes
            //response.StatusCode = HttpStatusCode.BadRequest;
            //

            if (response?.StatusCode == HttpStatusCode.OK || response?.StatusCode == HttpStatusCode.Created || response?.StatusCode == HttpStatusCode.Accepted)
            {
                //Each messageReceiver must complete it's own message or we'll get a lockToken error
                await messageReceiver.CompleteAsync(message.SystemProperties.LockToken);

                await SaveNotificationToDBAsync((int)response.StatusCode, message.MessageId, notification, appIdUri, clientUrl, bearerToken, true);
                log.LogInformation(string.Format("Saving to DB: Responsecode: {0} ResourceUrl: {1} MessageId: {2}", (int)response?.StatusCode, notification.ResourceURL, message.MessageId));
            }
            else
            {
                //Save notification to db
                await SaveNotificationToDBAsync((int)response.StatusCode, message.MessageId, notification, appIdUri, clientUrl, bearerToken, false);
                log.LogInformation(string.Format("Saving to DB: Responsecode: {0} ResourceUrl: {1} MessageId: {2}", (int)response?.StatusCode, notification.ResourceURL, message.MessageId));

                //Create Servicebus resend client
                var resendClient = new TopicClient(_connectionString, TopicName);
                var resendMessage = message.Clone();

                log.LogInformation("Cloning message for retry attempt");

                //Get number of retries attempted
                message.UserProperties.TryGetValue("RetryCount", out object rVal);
                int RetryCount = (int)rVal;

                if (RetryCount >= 12)
                {
                    try
                    {
                        //Deadletter as max retries exceeded
                        await messageReceiver.DeadLetterAsync(message.SystemProperties.LockToken, "MaxTriesExceeded", "Attempted to send notification to Endpoint 12 times & failed!");
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
                    //Schedule time for re-delivery attempt - UTC time + required number of seconds
                    int retrySecs = GetRetrySeconds(RetryCount);
                    resendMessage.ScheduledEnqueueTimeUtc = DateTime.UtcNow.AddSeconds(retrySecs);

                    //Increment retry count by 1
                    resendMessage.UserProperties["RetryCount"] = RetryCount + 1;
                    log.LogInformation(string.Format("Retrying message delivery Count {0}", RetryCount));

                    try
                    {
                        //Resend Message to the Topic
                        log.LogInformation("Attempting to resend message to the Topic");
                        await resendClient.SendAsync(resendMessage);
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
                    await messageReceiver.CompleteAsync(message.SystemProperties.LockToken);
                    log.LogInformation("Message successfully resent");
                }
            }

            log.LogInformation("Exiting PushToTouchpoint");
        }

        public static int GetRetrySeconds(int RetryCount)
        {
            switch (RetryCount)
            {
                case (0):
                    return 1;
                case (1):
                    return 2;
                case (2):
                    return 4;
                case (3):
                    return 8;
                case (4):
                    return 16;
                case (5):
                    return 32;
                case (6):
                    return 64;
                case (7):
                    return 128;
                case (8):
                    return 256;
                case (9):
                    return 512;
                case (10):
                    return 1024;
                case (11):
                    return 2048;
                default:
                    return 0;
            }
        }

        public static async Task SaveNotificationToDBAsync(int rspHttpCode, string MessageId, Notification rspNotification, string AppIdUri, string ClientUrl, string BearerToken, bool Success)
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

            try
            {
                await documentDbProvider.CreateNotificationAsync(DBNotification);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

}
