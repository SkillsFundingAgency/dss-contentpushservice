using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using System.Net.Http;
using NCS.DSS.ContentPushService.Models;
using Newtonsoft.Json;

namespace NCS.DSS.ContentPushService
{
    class MessagePushService
    {
        public async Task PushToTouchpoint(string Message)
        {
            HttpClient client = new HttpClient();
            MessageModel customer = JsonConvert.DeserializeObject<MessageModel>(Message);
            string ClientURL = "";

            var values = new Dictionary<string, string>
            {
               {    "Message",          "Customer modified notification"    },
               {    "CustomerId",       customer.CustomerGuid.ToString()    },
               {    "Resource URL",     customer.URL                        }
            };

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(ClientURL, content);
            var responseString = await response.Content.ReadAsStringAsync();
        }

    }
}
