using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using NCS.DSS.ContentPushService.Listeners.Models;
using Newtonsoft.Json;

namespace NCS.DSS.ContentPushService.PushService
{
    public class MessagePushService
    {
        public async Task PushToTouchpoint(string message)
        {
            var client = new HttpClient();
            var customer = JsonConvert.DeserializeObject<MessageModel>(message);
            const string clientUrl = "https://test.cognisoft.com/callback/api/Notification";

            var values = new Dictionary<string, string>
            {
               {    "CustomerId",       customer.CustomerGuid.ToString()    },
               {    "ResourceURL",     customer.URL                        },
               { "LastModifiedDate", customer.LastmodifiedDate.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)}
            };

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(clientUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();
        }

    }
}
