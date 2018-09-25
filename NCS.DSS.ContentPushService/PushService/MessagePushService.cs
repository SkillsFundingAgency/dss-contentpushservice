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

        public async Task PushToTouchpoint(string AppIdUri, string ClientUrl, BrokeredMessage message)
        {
            if (message == null)
            {
                return;
            }

            string appIdUri = ConfigurationManager.AppSettings[AppIdUri].ToString();
            var bearerToken = await AuthenticationHelper.GetAccessToken(appIdUri);
            string clientUrl = ConfigurationManager.AppSettings[ClientUrl].ToString();

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

            if (response?.StatusCode == HttpStatusCode.OK)
            {
                message.Complete();
                await SaveNotificationToDBAsync((int)response.StatusCode, message.MessageId, notification, appIdUri, clientUrl, bearerToken, true);
            }
            else
            {
                //Save notification to db
                await SaveNotificationToDBAsync((int)response.StatusCode, message.MessageId, notification, appIdUri, clientUrl, bearerToken, false);

                //Create Servicebus resend client
                var resendClient = TopicClient.CreateFromConnectionString(_connectionString, GetTopic(customer.TouchpointId));
                var resendMessage = message.Clone();

                //Schedule time for re-delivery attempt - UTC time + required number of seconds
                resendMessage.ScheduledEnqueueTimeUtc = DateTime.UtcNow.AddSeconds(GetRetrySeconds(message));

                //Increment retry count by 1
                message.Properties.TryGetValue("RetryNumber", out object rVal);
                int incRetryNumber = (int)rVal + 1;
                resendMessage.Properties["RetryNumber"] = incRetryNumber;

                //Resend Message to the Topic with new properties
                await resendClient.SendAsync(resendMessage);
                resendClient.Close();

                //Set original message to complete
                message.Complete();
            }
        }

        public static int GetRetrySeconds(BrokeredMessage message)
        {
            message.Properties.TryGetValue("RetryNumber", out object rVal);
            int retryNo = (int)rVal;

            //message.Properties.TryGetValue("RetryHttpStatusCode", out rVal);
            //string retryHttpStatusCode = (string)rVal;
            
            switch (retryNo)
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
                BearerToken = BearerToken,
                Success = Success,
                Notification = rspNotification,
                Timestamp = DateTime.Now
            };

            var documentDbProvider = new DocumentDBProvider();
            await documentDbProvider.CreateNotificationAsync(DBNotification);
        }


        private static string GetTopic(string touchPointId)
        {
            switch (touchPointId)
            {
                case "0000000101":
                    return "eastandbucks";
                case "0000000102":
                    return "eastandnorthampton";
                case "0000000103":
                    return "london";
                case "0000000104":
                    return "westmidsandstaffs";
                case "0000000105":
                    return "northwest";
                case "0000000106":
                    return "northeastandcumbria";
                case "0000000107":
                    return "southeast";
                case "0000000108":
                    return "southwestandoxford";
                case "0000000109":
                    return "yorkshireandhumber";
                case "0000000999":
                    return "careershelpline";


                ////////////////////////////////////
                ///////For test team use only///////
                case "9000000000":
                    return "dss-test-touchpoint-1";
                case "9111111111":
                    return "dss-test-touchpoint-2";
                case "9222222222":
                    return "dss-test-touchpoint-3";
                ////////////////////////////////////

                default:
                    return string.Empty;
            }

        }
    }

}
