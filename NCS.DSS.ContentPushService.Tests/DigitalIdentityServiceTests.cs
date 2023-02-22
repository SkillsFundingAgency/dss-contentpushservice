using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.ContentPushService.Constants;
using NCS.DSS.ContentPushService.Models;
using NCS.DSS.ContentPushService.Services;
using NCS.DSS.ContentPushService.Utils;
using Newtonsoft.Json;
using NUnit.Framework;

namespace NCS.DSS.ContentPushService.Tests;

[TestFixture]
public class DigitalIdentityServiceTests
{
    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<DigitialIdentityService>>();
        RequeueService = new Mock<IRequeueService>();
        DigitalIdentityClient = new Mock<IDigitalIdentityClient>();
        DigitalIdentityService =
            new DigitialIdentityService(RequeueService.Object, DigitalIdentityClient.Object, _logger.Object);
        MessageReceiver = new Mock<ServiceBusMessageActions>();
    }

    private IDigitialIdentityService DigitalIdentityService;
    private Mock<IDigitalIdentityClient> DigitalIdentityClient;
    private Mock<IRequeueService> RequeueService;
    private Mock<ILogger<DigitialIdentityService>> _logger;
    private Mock<ServiceBusMessageActions> MessageReceiver;

    private async Task<DigitalIdentityServiceActions> SendMessage(ServiceBusReceivedMessage request)
    {
        return await DigitalIdentityService.SendMessage("", request, MessageReceiver.Object);
    }

    [Test]
    public async Task Post_SignUpInvitation_SucceedsAndReturnsSuccessfullyActioned()
    {
        // Arrange
        var identity = new DigitalIdentity { CreateDigitalIdentity = true };
        DigitalIdentityClient
            .Setup(x => x.Post(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(true));

        var message =
            ServiceBusModelFactory.ServiceBusReceivedMessage(
                new BinaryData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity))));

        // Act
        var response = await SendMessage(message);

        // Assert
        Assert.AreEqual(DigitalIdentityServiceActions.SuccessfullyActioned, response);
    }

    [Test]
    public async Task Post_SignUpInvitation_FailsAndReturnsRequeued()
    {
        // Arrange
        var identity = new DigitalIdentity { CreateDigitalIdentity = true };
        DigitalIdentityClient
            .Setup(x => x.Post(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(false));
        RequeueService
            .Setup(x => x.RequeueItem(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ServiceBusReceivedMessage>()))
            .Returns(Task.FromResult(true));
        var msg = new Message(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity)));
        var message =
            ServiceBusModelFactory.ServiceBusReceivedMessage(
                new BinaryData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity))));
        // Act
        var response = await SendMessage(message);

        // Assert
        Assert.AreEqual(DigitalIdentityServiceActions.Requeued, response);
    }

    [Test]
    public async Task Post_SignUpInvitation_FailsAndDeadLettersMessage()
    {
        // Arrange
        var identity = new DigitalIdentity { CreateDigitalIdentity = true };
        DigitalIdentityClient
            .Setup(x => x.Post(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(false));
        RequeueService
            .Setup(x => x.RequeueItem(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ServiceBusReceivedMessage>()))
            .Returns(Task.FromResult(false));
        var message =
            ServiceBusModelFactory.ServiceBusReceivedMessage(
                new BinaryData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity))));
        // Act
        var response = await SendMessage(message);

        // Assert
        Assert.AreEqual(DigitalIdentityServiceActions.DeadLettered, response);
    }

    [Test]
    public async Task Delete_DeleteUser_SucceedsAndReturnsSuccessfullyActioned()
    {
        // Arrange
        var identity = new DigitalIdentity { DeleteDigitalIdentity = true };
        DigitalIdentityClient
            .Setup(x => x.Delete(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(true));
        var message =
            ServiceBusModelFactory.ServiceBusReceivedMessage(
                new BinaryData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity))));
        // Act
        var response = await SendMessage(message);

        // Assert
        Assert.AreEqual(DigitalIdentityServiceActions.SuccessfullyActioned, response);
    }

    [Test]
    public async Task Delete_DeleteUser_FailsAndDeadLettersMessage()
    {
        // Arrange
        var identity = new DigitalIdentity { DeleteDigitalIdentity = true };
        DigitalIdentityClient
            .Setup(x => x.Delete(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(false));
        RequeueService
            .Setup(x => x.RequeueItem(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ServiceBusReceivedMessage>()))
            .Returns(Task.FromResult(false));
        var message =
            ServiceBusModelFactory.ServiceBusReceivedMessage(
                new BinaryData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity))));
        // Act
        var response = await SendMessage(message);

        // Assert
        Assert.AreEqual(DigitalIdentityServiceActions.DeadLettered, response);
    }

    [Test]
    public void Generic_UnableToDetermineActionRequired_ThrowsException()
    {
        // Arrange
        var identity = new DigitalIdentity();
        DigitalIdentityClient
            .Setup(x => x.Post(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(false));
        RequeueService
            .Setup(x => x.RequeueItem(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ServiceBusReceivedMessage>()))
            .Returns(Task.FromResult(false));
        var message =
            ServiceBusModelFactory.ServiceBusReceivedMessage(
                new BinaryData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity))));


        // Act
        var ex = Assert.ThrowsAsync<Exception>(() => SendMessage(message));

        // Assert
        Assert.That(ex.Message,
            Is.EqualTo("Unable to determine if digital identity needs to be updated/created/deleted for customer: "));
    }

    [Test]
    public async Task Generic_MessageNull_ReturnsCouldNotAction()
    {
        // Arrange
        DigitalIdentityClient
            .Setup(x => x.Post(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(false));
        RequeueService
            .Setup(x => x.RequeueItem(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ServiceBusReceivedMessage>()))
            .Returns(Task.FromResult(false));


        // Act
        var response = await SendMessage(null);

        // Assert
        Assert.AreEqual(DigitalIdentityServiceActions.CouldNotAction, response);
    }

    [Test]
    public async Task Put_UpdateUser_SucceedsAndReturnsSuccessfullyActioned()
    {
        // Arrange
        var identity = new DigitalIdentity { UpdateDigitalIdentity = true };
        DigitalIdentityClient
            .Setup(x => x.Put(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(true));
        var message =
            ServiceBusModelFactory.ServiceBusReceivedMessage(
                new BinaryData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity))));
        // Act
        var response = await SendMessage(message);

        // Assert
        Assert.AreEqual(DigitalIdentityServiceActions.SuccessfullyActioned, response);
    }

    [Test]
    public async Task Put_UpdateUser_FailsAndDeadLettersMessage()
    {
        // Arrange
        var identity = new DigitalIdentity { UpdateDigitalIdentity = true };
        DigitalIdentityClient
            .Setup(x => x.Put(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(false));
        var message =
            ServiceBusModelFactory.ServiceBusReceivedMessage(
                new BinaryData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity))));
        // Act
        var response = await SendMessage(message);

        // Assert
        Assert.AreEqual(DigitalIdentityServiceActions.DeadLettered, response);
    }

    [Test]
    public async Task Put_UpdateUser_FailsAndReturnsRequeued()
    {
        // Arrange
        var identity = new DigitalIdentity { UpdateDigitalIdentity = true };
        DigitalIdentityClient
            .Setup(x => x.Post(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(false));
        RequeueService
            .Setup(x => x.RequeueItem(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ServiceBusReceivedMessage>()))
            .Returns(Task.FromResult(true));
        var message =
            ServiceBusModelFactory.ServiceBusReceivedMessage(
                new BinaryData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity))));

        // Act
        var response = await SendMessage(message);

        // Assert
        Assert.AreEqual(DigitalIdentityServiceActions.Requeued, response);
    }

    [Test]
    public async Task Post_ChangeEmail_SucceedsAndReturnsSuccessfullyActioned()
    {
        // Arrange
        var identity = new DigitalIdentity { ChangeEmailAddress = true };
        DigitalIdentityClient
            .Setup(x => x.Post(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(true));
        var message =
            ServiceBusModelFactory.ServiceBusReceivedMessage(
                new BinaryData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity))));
        // Act
        var response = await SendMessage(message);

        // Assert
        Assert.AreEqual(DigitalIdentityServiceActions.SuccessfullyActioned, response);
    }

    [Test]
    public async Task Post_ChangeEmail_FailsAndDeadLettersMessage()
    {
        // Arrange
        var identity = new DigitalIdentity { ChangeEmailAddress = true };
        DigitalIdentityClient
            .Setup(x => x.Post(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(false));
        var message =
            ServiceBusModelFactory.ServiceBusReceivedMessage(
                new BinaryData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity))));
        // Act
        var response = await SendMessage(message);

        // Assert
        Assert.AreEqual(DigitalIdentityServiceActions.DeadLettered, response);
    }

    [Test]
    public async Task Post_ChangeEmail_FailsAndReturnsRequeued()
    {
        // Arrange
        var identity = new DigitalIdentity { ChangeEmailAddress = true };
        DigitalIdentityClient
            .Setup(x => x.Post(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(false));
        RequeueService
            .Setup(x => x.RequeueItem(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ServiceBusReceivedMessage>()))
            .Returns(Task.FromResult(true));
        var message =
            ServiceBusModelFactory.ServiceBusReceivedMessage(
                new BinaryData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity))));
        // Act
        var response = await SendMessage(message);

        // Assert
        Assert.AreEqual(DigitalIdentityServiceActions.Requeued, response);
    }
}