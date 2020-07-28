using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NCS.DSS.ContentPushService.Utils
{
    public class DigitalIdentityClient : IDigitalIdentityClient
    {
        public readonly IHttpClientFactory _httpclientFactory;
        private readonly ILogger<DigitalIdentityClient> _logger;

        public DigitalIdentityClient(IHttpClientFactory httpclientFactory, ILogger<DigitalIdentityClient> logger)
        {
            _httpclientFactory = httpclientFactory;
            _logger = logger;
        }

        public async Task<bool> Post<T>(T t, string endpoint)
        {
            using (var client = _httpclientFactory.CreateClient("AzureB2C"))
            using (var request = new HttpRequestMessage(HttpMethod.Post, endpoint))
            {
                var json = JsonConvert.SerializeObject(t);
                using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    request.Content = stringContent;
                    using (var response = await client.SendAsync(request))
                    {
                        return response.StatusCode == System.Net.HttpStatusCode.Created;
                    }
                }
            }
        }

        public async Task<bool> Patch<T>(T t, string endpoint)
        {
            using (var client = _httpclientFactory.CreateClient("AzureB2C"))
            using (var request = new HttpRequestMessage(HttpMethod.Patch, endpoint))
            {
                var json = JsonConvert.SerializeObject(t);
                using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    request.Content = stringContent;
                    using (var response = await client.SendAsync(request))
                    {
                        return response.StatusCode == System.Net.HttpStatusCode.OK;
                    }
                }
            }
        }

        public async Task<bool> Delete<T>(T t, string endpoint)
        {
            using (var client = _httpclientFactory.CreateClient("AzureB2C"))
            using (var request = new HttpRequestMessage(HttpMethod.Delete, endpoint))
            {
                var json = JsonConvert.SerializeObject(t);
                using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    request.Content = stringContent;
                    using (var response = await client.SendAsync(request))
                    {
                        return response.StatusCode == System.Net.HttpStatusCode.OK;
                    }
                }
            }
        }
    }
}
