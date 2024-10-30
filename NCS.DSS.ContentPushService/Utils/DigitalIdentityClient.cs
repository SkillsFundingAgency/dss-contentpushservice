using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;

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

        public async Task<bool> Post(string customerId, string objectId, string body, string endpoint)
        {
            var response = await Send(HttpMethod.Post, endpoint, body);
            var respstr = await GetResponseContent(response);
            var isSuccess = IsSuccessfulResponse(response, HttpStatusCode.OK);
            if (!isSuccess)
                _logger.LogInformation($"Failed to post customerId: {customerId}, ObjectId: {objectId} - Status: {response?.StatusCode} response is: {respstr} ");
            return isSuccess;
        }

        public async Task<bool> Patch(string customerId, string objectId, string body, string endpoint)
        {
            var response = await Send(HttpMethod.Patch, endpoint, body);
            var respstr = await GetResponseContent(response);
            var isSuccess = IsSuccessfulResponse(response, HttpStatusCode.OK);
            if (!isSuccess)
                _logger.LogInformation($"Failed to Patch json {body} - Status: {response?.StatusCode}  response is: {respstr} ");
            return isSuccess;
        }

        public async Task<bool> Delete(string customerId, string objectId, string body, string endpoint)
        {
            var response = await Send(HttpMethod.Delete, endpoint, body);
            var respstr = await GetResponseContent(response);
            var isSuccess = IsSuccessfulResponse(response, HttpStatusCode.OK);
            if (!isSuccess)
                _logger.LogInformation($"Failed to delete customerId: {customerId}, objectId: {objectId} - Status: {response?.StatusCode}  response is : {respstr} ");
            return isSuccess;
        }

        public async Task<bool> Put(string customerId, string objectId, string body, string endpoint)
        {
            var response = await Send(HttpMethod.Put, endpoint, body);
            var respstr = await GetResponseContent(response);
            var isSuccess = IsSuccessfulResponse(response, HttpStatusCode.OK);
            if (!isSuccess)
                _logger.LogInformation($"Failed to put customerId: {customerId}, objectId: {objectId} - Status: {response?.StatusCode}  response is : {respstr} ");
            return isSuccess;
        }

        private async Task<HttpResponseMessage> Send(HttpMethod verb, string endpoint, string body)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return null;
        }

        private bool IsSuccessfulResponse(HttpResponseMessage resp, HttpStatusCode expectedResult)
        {
            return resp?.StatusCode == expectedResult;
        }

        private async Task<string> GetResponseContent(HttpResponseMessage resp)
        {
            if (resp != null)
                return await resp.Content.ReadAsStringAsync();
            return string.Empty;
        }
    }
}
