using System.Collections.Generic;
using System.Globalization;
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

            var values = new Dictionary<string, string>
            {
               {    "CustomerId",       customer.CustomerGuid.ToString()    },
               {    "ResourceURL",     customer.URL                        },
               { "LastModifiedDate", customer.LastModifiedDate.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)}
            };

            var content = new FormUrlEncodedContent(values);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            var response = await client.PostAsync(clientUrl, content);

            var responseString = await response.Content.ReadAsStringAsync();
        }

    }
}
