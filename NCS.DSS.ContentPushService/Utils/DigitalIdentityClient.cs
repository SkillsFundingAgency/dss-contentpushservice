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
            _logger.LogInformation("Sending post to endpoint");
            var response = await Send(HttpMethod.Post, endpoint, body);
            _logger.LogInformation("Recieved response from endpoint");
            var respstr = await GetResponseContent(response);
            var isSuccess = IsSuccessfulResponse(response, HttpStatusCode.OK);
            if (!isSuccess)
            {
                _logger.LogWarning($"Failed to Post customerId: {customerId}, ObjectId: {objectId} - Status: {response?.StatusCode} response is: {respstr} ");
            }
            else
            {
                _logger.LogInformation($"Successfully Posted customerId: {customerId}, ObjectId: {objectId} - Status: {response?.StatusCode} response is: {respstr} ");
            }
            return isSuccess;
        }

        public async Task<bool> Patch(string customerId, string objectId, string body, string endpoint)
        {
            _logger.LogInformation("Sending patch to endpoint");
            var response = await Send(HttpMethod.Patch, endpoint, body);
            _logger.LogInformation("Recieved response from endpoint");
            var respstr = await GetResponseContent(response);
            var isSuccess = IsSuccessfulResponse(response, HttpStatusCode.OK);
            if (!isSuccess)
            {
                _logger.LogWarning($"Failed to Patch json {body} - Status: {response?.StatusCode}  response is: {respstr} ");
            }
            else
            {
                _logger.LogInformation($"Successfully Patched json {body} - Status: {response?.StatusCode}  response is: {respstr} ");
            }
            return isSuccess;
        }

        public async Task<bool> Delete(string customerId, string objectId, string body, string endpoint)
        {
            _logger.LogInformation("Sending Delete request to endpoint");
            var response = await Send(HttpMethod.Delete, endpoint, body);
            _logger.LogInformation("Recieved response from endpoint");
            var respstr = await GetResponseContent(response);
            var isSuccess = IsSuccessfulResponse(response, HttpStatusCode.OK);
            if (!isSuccess)
            {
                _logger.LogWarning($"Failed to Delete customerId: {customerId}, objectId: {objectId} - Status: {response?.StatusCode}  response is : {respstr} ");
            }
            else
            {
                _logger.LogInformation($"Successfully Deleted customerId: {customerId}, objectId: {objectId} - Status: {response?.StatusCode}  response is : {respstr} ");
            }
            return isSuccess;
        }

        public async Task<bool> Put(string customerId, string objectId, string body, string endpoint)
        {
            _logger.LogInformation("Sending Put request to endpoint");
            var response = await Send(HttpMethod.Put, endpoint, body);
            _logger.LogInformation("Recieved response from endpoint");
            var respstr = await GetResponseContent(response);
            var isSuccess = IsSuccessfulResponse(response, HttpStatusCode.OK);
            if (!isSuccess)
            {
                _logger.LogWarning($"Failed to Put customerId: {customerId}, objectId: {objectId} - Status: {response?.StatusCode}  response is : {respstr} ");
            }
            else
            {
                _logger.LogInformation($"Successfullyed Putted customerId: {customerId}, objectId: {objectId} - Status: {response?.StatusCode}  response is : {respstr} ");
            }
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
                _logger.LogError("Failed to send request to client.", ex.Message);
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
