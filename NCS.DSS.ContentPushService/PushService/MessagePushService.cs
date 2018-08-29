using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using NCS.DSS.ContentPushService.Models;
using Newtonsoft.Json;

namespace NCS.DSS.ContentPushService.PushService
{
    public class MessagePushService
    {
        public async Task PushToTouchpoint(BrokeredMessage message, string clientUrl, string bearerToken)
        {
            if (message == null)
                return;

            var client = new HttpClient();
            var body = new StreamReader(message.GetBody<Stream>(), Encoding.UTF8).ReadToEnd();

            var customer = JsonConvert.DeserializeObject<MessageModel>(body);

            if (customer == null)
                return;

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
            
            await client.PostAsync(clientUrl, byteContent);
        }

    }
}
