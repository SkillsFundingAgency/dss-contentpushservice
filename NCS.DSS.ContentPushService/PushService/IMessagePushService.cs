﻿using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.ContentPushService.PushService
{
    public interface IMessagePushService
    {
        Task PushToTouchpoint(string AppIdUri, string ClientUrl, Message message, string TopicName, MessageReceiver messageReceiver, ILogger log);
    }
}