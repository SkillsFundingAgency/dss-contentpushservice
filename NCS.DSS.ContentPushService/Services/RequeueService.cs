using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.ContentPushService.Models;

namespace NCS.DSS.ContentPushService.Services;

public class RequeueService : IRequeueService
{
    IOptions<ContentPushServiceConfigurationSettings> _configurationSettings;
    private readonly ILogger<RequeueService> _logger;

    public RequeueService(IOptions<ContentPushServiceConfigurationSettings> configOptions, ILogger<RequeueService> logger)
    {
        _configurationSettings = configOptions;
        _logger = logger;
    }

    public async Task<bool> RequeueItem(string topicName, int maxRetryCount,
        ServiceBusReceivedMessage serviceBusReceivedMessage)
    {
        var config = _configurationSettings.Value;
        var connectionString = config.ServiceBusConnectionString;

        await using var resendClient = new ServiceBusClient(connectionString);
        await using var sbSender = resendClient.CreateSender(topicName);
        var resendMessage = new ServiceBusMessage(serviceBusReceivedMessage);
        serviceBusReceivedMessage.ApplicationProperties.TryGetValue("RetryCount", out var rVal);

        var retryCount = (int)rVal;

        //Schedule time for re-delivery attempt - UTC time + required number of seconds
        if (retryCount < maxRetryCount)
        {
            var retrySecs = GetRetrySeconds(retryCount);
            resendMessage.ScheduledEnqueueTime = DateTime.UtcNow.AddSeconds(retrySecs);
            resendMessage.ApplicationProperties["RetryCount"] = retryCount + 1;

            _logger.LogInformation(
                $"Attempting to resend message, Attempt:{retryCount} to the Topic {resendMessage.MessageId}");
            await sbSender.SendMessageAsync(resendMessage);
            return true;
        }

        return false;
    }

    public int GetRetrySeconds(int RetryCount)
    {
        switch (RetryCount)
        {
            case 0:
                return 1;
            case 1:
                return 2;
            case 2:
                return 4;
            case 3:
                return 8;
            case 4:
                return 16;
            case 5:
                return 32;
            case 6:
                return 64;
            case 7:
                return 128;
            case 8:
                return 256;
            case 9:
                return 512;
            case 10:
                return 1024;
            case 11:
                return 2048;
            default:
                return 0;
        }
    }
}