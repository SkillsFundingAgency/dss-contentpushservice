using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.ContentPushService.Constants;
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
    public class DigitalIdentityServiceTests
    {
        private IDigitialIdentityService DigitalIdentityService;
        private Mock<IDigitalIdentityClient> DigitalIdentityClient;
        private Mock<IRequeueService> RequeueService;
        private DigitalIdentityTopicListener DigitalIdentityTopicListener;
        private Mock<ILogger> _logger;
        private Mock<IMessageReceiverService> MessageReceiver;
        private DigitialIdentityService _diService;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger>();
            RequeueService = new Mock<IRequeueService>();
            DigitalIdentityClient = new Mock<IDigitalIdentityClient>();
            DigitalIdentityService = new DigitialIdentityService(RequeueService.Object,  DigitalIdentityClient.Object);
            MessageReceiver = new Mock<IMessageReceiverService>();
            DigitalIdentityTopicListener = new DigitalIdentityTopicListener(DigitalIdentityService, MessageReceiver.Object);
            DigitalIdentityService = new DigitialIdentityService(RequeueService.Object, DigitalIdentityClient.Object);

        }


        #region Create Digital Identity
        private async Task<DigitalIdentityServiceActions> SendMessage( Message request)
        {
            return await DigitalIdentityService.SendMessage("", request, MessageReceiver.Object, _logger.Object);
        }

        [Test]
        public async Task Post_SignUpInvitation_SucceedsAndReturnsSuccessfullyActioned()
        {
            // Arrange
            var identity = new DigitalIdentity() { CreateDigitalIdentity = true} ;
            DigitalIdentityClient.Setup(x => x.Post(It.IsAny<object>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            var msg = new Message(System.Text.Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(identity)));

            // Act
            var response = await SendMessage(msg);

            // Assert
            Assert.AreEqual(DigitalIdentityServiceActions.SuccessfullyActioned, response);
        }

        [Test]
        public async Task Post_SignUpInvitation_FailsAndReturnsRequeued()
        {
            // Arrange
            var identity = new DigitalIdentity() { CreateDigitalIdentity = true };
            DigitalIdentityClient.Setup(x => x.Post(It.IsAny<DigitalIdentity>(), It.IsAny<string>())).Returns(Task.FromResult(false));
            RequeueService.Setup(x => x.RequeueItem(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Message>(), It.IsAny<ILogger>())).Returns(Task.FromResult(true));
            var msg = new Message(System.Text.Encoding.Default.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(identity)));

             // Act
             var response = await SendMessage(msg);

            // Assert
            Assert.AreEqual(DigitalIdentityServiceActions.Requeued, response);
        }

        [Test]
        public async Task Post_SignUpInvitation_FailsAndDeadLettersMessage()
        {
            // Arrange
            var identity = new DigitalIdentity() { CreateDigitalIdentity = true };
            DigitalIdentityClient.Setup(x => x.Post(It.IsAny<DigitalIdentity>(), It.IsAny<string>())).Returns(Task.FromResult(false));
            RequeueService.Setup(x => x.RequeueItem(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Message>(), It.IsAny<ILogger>())).Returns(Task.FromResult(false));
            var msg = new Message(System.Text.Encoding.Default.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(identity)));

            // Act
            var response = await SendMessage(msg);

            // Assert
            Assert.AreEqual(DigitalIdentityServiceActions.DeadLettered, response);
        }

        [Test]
        public void UnableToDetermineActionRequired_Throws_Exception()
        {
            // Arrange
            var identity = new DigitalIdentity() {  };
            DigitalIdentityClient.Setup(x => x.Post(It.IsAny<DigitalIdentity>(), It.IsAny<string>())).Returns(Task.FromResult(false));
            RequeueService.Setup(x => x.RequeueItem(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Message>(), It.IsAny<ILogger>())).Returns(Task.FromResult(false));
            var msg = new Message(System.Text.Encoding.Default.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(identity)));
            

            // Act
            var ex = Assert.ThrowsAsync<Exception>(() => SendMessage(msg));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Unable to determine if digital identity needs to be updated/created/deleted for customer: "));
        }
        #endregion

        #region DeleteUser
        //DeleteUser WIP
        #endregion

        #region UpdateUser
        //UpdateUser WIP
        #endregion

        #region ChangeEmail
        //ChangeEmail WIP
        #endregion

    }
}
