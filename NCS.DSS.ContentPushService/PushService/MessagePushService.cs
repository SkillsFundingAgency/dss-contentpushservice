using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NCS.DSS.ContentPushService.Models;
using Newtonsoft.Json;

namespace NCS.DSS.ContentPushService.PushService
{
    public class MessagePushService
    {
        public async Task PushToTouchpoint(string message, string clientUrl, string bearerToken)
        {
            var client = new HttpClient();
            var customer = JsonConvert.DeserializeObject<MessageModel>(message);

            var notification = new Notification
            {
                CustomerId = customer.CustomerGuid.GetValueOrDefault(),
                ResourceURL = customer.URL,
                LastModifiedDate = customer.LastModifiedDate.GetValueOrDefault()
            };

            var content = JsonConvert.SerializeObject(notification);

            var buffer = System.Text.Encoding.UTF8.GetBytes(content);
            var byteContent = new ByteArrayContent(buffer);

            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            
            var response = await client.PostAsync(clientUrl, byteContent);

            var responseContent = response.Content;
        }

    }
}
