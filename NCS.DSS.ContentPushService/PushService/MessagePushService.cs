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

            if (response?.StatusCode == HttpStatusCode.OK)
            {
                message.Complete();
                await SaveNotificationToDBAsync((int)response.StatusCode, message.MessageId, notification, appIdUri, clientUrl, bearerToken, true);
            }
            else
            {
                await SaveNotificationToDBAsync((int)response.StatusCode, message.MessageId, notification, appIdUri, clientUrl, bearerToken, false);
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

    }

}
