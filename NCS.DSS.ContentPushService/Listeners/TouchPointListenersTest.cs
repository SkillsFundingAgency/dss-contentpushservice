using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.ContentPushService.Listeners
{
    public class TouchPointListenersTest
    {
        private readonly IListenersHelper _listenersHelper;
        private const string ServiceBusConnectionString = "ServiceBusConnectionString";
        public const string TP_9000000001 = "9000000001";
        public const string TP_9000000002 = "9000000002";
        public const string TP_9000000003 = "9000000003";

        public TouchPointListenersTest(IListenersHelper listenersHelper)
        {
            _listenersHelper = listenersHelper;
        }

        [FunctionName("TOUCHPOINT_" + TP_9000000001)]
        public async Task TouchPoint_9000000001(
            [ServiceBusTrigger(TP_9000000001, TP_9000000001, Connection = ServiceBusConnectionString)]
            Message serviceBusMessage, MessageReceiver messageReceiver, ILogger log)
        {
            await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_9000000001, messageReceiver, log);
        }

        [FunctionName("TOUCHPOINT_" + TP_9000000002)]
        public async Task TouchPoint_9000000002(
            [ServiceBusTrigger(TP_9000000002, TP_9000000002, Connection = ServiceBusConnectionString)]
            Message serviceBusMessage, MessageReceiver messageReceiver, ILogger log)
        {
            await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_9000000002, messageReceiver, log);
        }

        [FunctionName("TOUCHPOINT_" + TP_9000000003)]
        public async Task TouchPoint_9000000003(
            [ServiceBusTrigger(TP_9000000003, TP_9000000003, Connection = ServiceBusConnectionString)]
            Message serviceBusMessage, MessageReceiver messageReceiver, ILogger log)
        {
            await _listenersHelper.SendMessageAsync(serviceBusMessage, TP_9000000003, messageReceiver, log);
        }
    }
}
