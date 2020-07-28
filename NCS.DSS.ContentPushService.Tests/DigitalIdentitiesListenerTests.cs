using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.ContentPushService.Listeners;
using NCS.DSS.ContentPushService.Models;
using NCS.DSS.ContentPushService.Services;
using NCS.DSS.ContentPushService.Utils;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace NCS.DSS.ContentPushService.Tests
{
    [TestFixture]
    public class DigitalIdentitiesListenerTests
    {
        private IDigitialIdentityService DigitalIdentityService;
        private Mock<IDigitalIdentityClient> DigitalIdentityClient;
        private Mock<IRequeueService> RequeueService;
        private DigitalIdentityTopicListener DigitalIdentityTopicListener;
        private Mock<ILogger> _logger;
        private Mock<MessageReceiver> MessageReceiver;

        [SetUp]
        public void Setup()
        {
            var s = new MessageReceiver(null);
            _logger = new Mock<ILogger>();
            RequeueService = new Mock<IRequeueService>();
            DigitalIdentityClient = new Mock<IDigitalIdentityClient>();
            DigitalIdentityService = new DigitialIdentityService(RequeueService.Object, _logger.Object, DigitalIdentityClient.Object);
            DigitalIdentityTopicListener = new DigitalIdentityTopicListener(DigitalIdentityService);
            MessageReceiver = new Mock<MessageReceiver>(It.IsAny<string>());
        }

        private async Task RunFunction( Message request)
        {
            await DigitalIdentityTopicListener.RunAsync(
                request,
                MessageReceiver.Object,
                _logger.Object
            );
        }

        [Test]
        public async Task GivenDigitalIdentity_Post_Succeeds()
        {
            // Arrange
            var identity = new DigitalIdentity();
            DigitalIdentityClient.Setup(x => x.Post(It.IsAny<DigitalIdentity>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            var msg = new Message(System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(identity)));

            // Act
            await RunFunction(msg);

            // Assert
            Mock.Get(RequeueService.Object).Verify(x => x.RequeueItem(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Message>()), Times.Never);
        }

        [Test]
        public async Task GivenDigitalIdentity_Post_FailsAndRequeuesMessage()
        {
            // Arrange
            var identity = new DigitalIdentity();
            DigitalIdentityClient.Setup(x => x.Post(It.IsAny<DigitalIdentity>(), It.IsAny<string>())).Returns(Task.FromResult(false));
            var msg = new Message(System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(identity)));
           
            // Act
            await RunFunction(msg);

            // Assert
            Mock.Get(RequeueService.Object).Verify(x => x.RequeueItem(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Message>()), Times.Once);
        }
    }
}
