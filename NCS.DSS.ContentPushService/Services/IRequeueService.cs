﻿using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace NCS.DSS.ContentPushService.Services
{
    public interface IRequeueService
    {
        Task<bool> RequeueItem(string topicName, string connectionString, int maxRetryCount, Message message, ILogger logger);

    }
}
