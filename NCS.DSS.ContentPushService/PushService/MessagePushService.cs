using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentPushService.Auth;
using NCS.DSS.ContentPushService.Cosmos.Provider;
using NCS.DSS.ContentPushService.Models;
using Newtonsoft.Json;

namespace NCS.DSS.ContentPushService.PushService
{
    public class MessagePushService
    {
        readonly string _connectionString = ConfigurationManager.AppSettings["ServiceBusConnectionString"];

        public async Task PushToTouchpoint(string AppIdUri, string ClientUrl, BrokeredMessage message, string TopicName)
        {
            if (message == null)
            {
                return;
            }

            string appIdUri = ConfigurationManager.AppSettings[AppIdUri].ToString();

            if (appIdUri == null)
                throw new Exception("AppIdUri: " + AppIdUri + " does not exist!");
                        
            var bearerToken = await AuthenticationHelper.GetAccessToken(appIdUri);

            string clientUrl = ConfigurationManager.AppSettings[ClientUrl].ToString();
            if (clientUrl == null)
                throw new Exception("ClientUrl: " + clientUrl + " does not exist!");
            
            var client = new HttpClient();
            var body = new StreamReader(message.GetBody<Stream>(), Encoding.UTF8).ReadToEnd();

            var customer = JsonConvert.DeserializeObject<MessageModel>(body);

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

            var content = JsonConvert.SerializeObject(notification);
            var buffer = Encoding.UTF8.GetBytes(content);
            var byteContent = new ByteArrayContent(buffer);

            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            client.DefaultRequestHeaders.Add("OriginatingTouchpointId", customer.TouchpointId);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            var response = await client.PostAsync(clientUrl, byteContent);

            //For testing purposes
            //response.StatusCode = HttpStatusCode.BadRequest;
            //

            if (response?.StatusCode == HttpStatusCode.OK || response?.StatusCode == HttpStatusCode.Created || response?.StatusCode == HttpStatusCode.Accepted) 
            {
                message.Complete();
                await SaveNotificationToDBAsync((int)response.StatusCode, message.MessageId, notification, appIdUri, clientUrl, bearerToken, true);
            }
            else
            {
                //Save notification to db
                await SaveNotificationToDBAsync((int)response.StatusCode, message.MessageId, notification, appIdUri, clientUrl, bearerToken, false);

                //Create Servicebus resend client
                var resendClient = TopicClient.CreateFromConnectionString(_connectionString, TopicName);
                var resendMessage = message.Clone();

                //Get number of retries attempted
                message.Properties.TryGetValue("RetryCount", out object rVal);
                int RetryCount = (int)rVal;
                
                if (RetryCount >= 12)
                {
                    //Deadletter as max retries exceeded
                    await resendMessage.DeadLetterAsync("MaxTriesExceeded", "DSS Attempted to send notification to ABC Endpoint and reached retry limit!");
                    resendClient.Close();
                    await message.CompleteAsync();
                }
                else
                {
                    //Schedule time for re-delivery attempt - UTC time + required number of seconds
                    int retrySecs = GetRetrySeconds(RetryCount);
                    resendMessage.ScheduledEnqueueTimeUtc = DateTime.UtcNow.AddSeconds(retrySecs);

                    //Increment retry count by 1
                    resendMessage.Properties["RetryCount"] = RetryCount + 1;

                    //Resend Message to the Topic
                    await resendClient.SendAsync(resendMessage);
                    resendClient.Close();

                    //Complete original message
                    await message.CompleteAsync();
                }
            }
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

        public static async Task SaveNotificationToDBAsync(int rspHttpCode,
                                                string MessageId,
                                                Notification rspNotification,
                                                string AppIdUri,
                                                string ClientUrl,
                                                string BearerToken,
                                                bool Success)
        {

            var DBNotification = new DBNotification
            {
                MessageId = MessageId,
                HttpCode = rspHttpCode,
                AppIdUri = AppIdUri,
                ClientUrl = ClientUrl,
                BearerToken = "",
                Success = Success,
                Notification = rspNotification,
                Timestamp = DateTime.Now
            };

            var documentDbProvider = new DocumentDBProvider();
            await documentDbProvider.CreateNotificationAsync(DBNotification);
        }

    }

}
