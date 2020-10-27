using Microsoft.Extensions.Logging;
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

        public async Task<bool> Post(string body, string endpoint)
        {
            var response = await Send(HttpMethod.Post, endpoint, body);
            var respstr = await response.Content.ReadAsStringAsync();
            var isSuccess = response.StatusCode == System.Net.HttpStatusCode.OK;
            if (!isSuccess)
                _logger.LogInformation($"Failed to post json {body} - Status: {response.StatusCode} response is: {respstr} ");
            return isSuccess;
        }

        public async Task<bool> Patch(string body, string endpoint)
        {
            var response = await Send(HttpMethod.Patch, endpoint, body);
            var respstr = await response.Content.ReadAsStringAsync();
            var isSuccess = response.StatusCode == System.Net.HttpStatusCode.OK;
            if (!isSuccess)
                _logger.LogInformation($"Failed to Patch json {body} - Status: {response.StatusCode}  response is: {respstr} ");
            return isSuccess;
        }

        public async Task<bool> Delete(string body, string endpoint)
        {
            var response = await Send(HttpMethod.Delete, endpoint, body);
            var respstr = await response.Content.ReadAsStringAsync();
            var isSuccess = response.StatusCode == System.Net.HttpStatusCode.OK;
            if (!isSuccess)
                _logger.LogInformation($"Failed to delete json {body} - Status: {response.StatusCode}  response is : {respstr} ");
            return isSuccess;
        }

        public async Task<bool> Put(string body, string endpoint)
        {
            var response = await Send(HttpMethod.Put, endpoint, body);
            var respstr = await response.Content.ReadAsStringAsync();
            var isSuccess = response.StatusCode == System.Net.HttpStatusCode.OK;
            if (!isSuccess)
                _logger.LogInformation($"Failed to put json {body} - Status: {response.StatusCode}  response is : {respstr} ");
            return isSuccess;
        }

        private async  Task<HttpResponseMessage> Send(HttpMethod verb, string endpoint, string body)
        {
            using (var client = _httpclientFactory.CreateClient("AzureB2C"))
            using (var request = new HttpRequestMessage(verb, endpoint))
            {
                using (var stringContent = new StringContent(body, Encoding.UTF8, "application/json"))
                {
                    request.Content = stringContent;
                    return await client.SendAsync(request);
                }
            }
        }
    }
}
