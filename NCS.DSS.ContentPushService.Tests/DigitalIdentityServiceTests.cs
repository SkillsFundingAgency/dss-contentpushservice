//using Azure.Messaging.ServiceBus;
//using Microsoft.Azure.Functions.Worker;
//using Microsoft.Extensions.Logging;
//using Moq;
//using NCS.DSS.ContentPushService.Constants;
//using NCS.DSS.ContentPushService.Models;
//using NCS.DSS.ContentPushService.Services;
//using NCS.DSS.ContentPushService.Utils;
//using Newtonsoft.Json;
//using NUnit.Framework;
//using System;
//using System.Text;
//using System.Threading.Tasks;

//namespace NCS.DSS.ContentPushService.Tests;

//[TestFixture]
//public class DigitalIdentityServiceTests
//{
//    [SetUp]
//    public void Setup()
//    {
//        RequeueService = new Mock<IRequeueService>();
//        DigitalIdentityClient = new Mock<IDigitalIdentityClient>();
        
//        MessageReceiver = new Mock<ServiceBusMessageActions>();
//    }

//    private Mock<IDigitalIdentityClient> DigitalIdentityClient;
//    private Mock<IRequeueService> RequeueService;
//    private Mock<ServiceBusMessageActions> MessageReceiver;

//    private async Task<DigitalIdentityServiceActions> SendMessage(ServiceBusReceivedMessage request)
//    {
//        return await DigitalIdentityService.SendMessage("", request, MessageReceiver.Object);
//    }

//    [Test]
//    public async Task Post_SignUpInvitation_SucceedsAndReturnsSuccessfullyActioned()
//    {
//        // Arrange
//        var identity = new DigitalIdentity { CreateDigitalIdentity = true };
//        DigitalIdentityClient
//            .Setup(x => x.Post(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
//            .Returns(Task.FromResult(true));

//        var message =
//            ServiceBusModelFactory.ServiceBusReceivedMessage(
//                new BinaryData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity))));

//        // Act
//        var response = await SendMessage(message);

//        // Assert
//        Assert.That(DigitalIdentityServiceActions.SuccessfullyActioned, Is.EqualTo(response));
//    }

//    [Test]
//    public async Task Post_SignUpInvitation_FailsAndReturnsRequeued()
//    {
//        // Arrange
//        var identity = new DigitalIdentity { CreateDigitalIdentity = true };
//        DigitalIdentityClient
//            .Setup(x => x.Post(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
//            .Returns(Task.FromResult(false));
//        RequeueService
//            .Setup(x => x.RequeueItem(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ServiceBusReceivedMessage>()))
//            .Returns(Task.FromResult(true));
//        var msg = new ServiceBusMessage(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity)));
//        var message =
//            ServiceBusModelFactory.ServiceBusReceivedMessage(
//                new BinaryData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity))));
//        // Act
//        var response = await SendMessage(message);

//        // Assert
//        Assert.That(DigitalIdentityServiceActions.Requeued, Is.EqualTo(response));
//    }

//    [Test]
//    public async Task Post_SignUpInvitation_FailsAndDeadLettersMessage()
//    {
//        // Arrange
//        var identity = new DigitalIdentity { CreateDigitalIdentity = true };
//        DigitalIdentityClient
//            .Setup(x => x.Post(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
//            .Returns(Task.FromResult(false));
//        RequeueService
//            .Setup(x => x.RequeueItem(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ServiceBusReceivedMessage>()))
//            .Returns(Task.FromResult(false));
//        var message =
//            ServiceBusModelFactory.ServiceBusReceivedMessage(
//                new BinaryData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity))));
//        // Act
//        var response = await SendMessage(message);

//        // Assert
//        Assert.That(DigitalIdentityServiceActions.DeadLettered, Is.EqualTo(response));
//    }

//    [Test]
//    public async Task Delete_DeleteUser_SucceedsAndReturnsSuccessfullyActioned()
//    {
//        // Arrange
//        var identity = new DigitalIdentity { DeleteDigitalIdentity = true };
//        DigitalIdentityClient
//            .Setup(x => x.Delete(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
//            .Returns(Task.FromResult(true));
//        var message =
//            ServiceBusModelFactory.ServiceBusReceivedMessage(
//                new BinaryData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity))));
//        // Act
//        var response = await SendMessage(message);

//        // Assert
//        Assert.That(DigitalIdentityServiceActions.SuccessfullyActioned, Is.EqualTo(response));
//    }

//    [Test]
//    public async Task Delete_DeleteUser_FailsAndDeadLettersMessage()
//    {
//        // Arrange
//        var identity = new DigitalIdentity { DeleteDigitalIdentity = true };
//        DigitalIdentityClient
//            .Setup(x => x.Delete(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
//            .Returns(Task.FromResult(false));
//        RequeueService
//            .Setup(x => x.RequeueItem(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ServiceBusReceivedMessage>()))
//            .Returns(Task.FromResult(false));
//        var message =
//            ServiceBusModelFactory.ServiceBusReceivedMessage(
//                new BinaryData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity))));
//        // Act
//        var response = await SendMessage(message);

//        // Assert
//        Assert.That(DigitalIdentityServiceActions.DeadLettered, Is.EqualTo(response));
//    }

//    [Test]
//    public void Generic_UnableToDetermineActionRequired_ThrowsException()
//    {
//        // Arrange
//        var identity = new DigitalIdentity();
//        DigitalIdentityClient
//            .Setup(x => x.Post(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
//            .Returns(Task.FromResult(false));
//        RequeueService
//            .Setup(x => x.RequeueItem(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ServiceBusReceivedMessage>()))
//            .Returns(Task.FromResult(false));
//        var message =
//            ServiceBusModelFactory.ServiceBusReceivedMessage(
//                new BinaryData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity))));


//        // Act
//        var ex = Assert.ThrowsAsync<Exception>(() => SendMessage(message));

//        // Assert
//        Assert.That(ex.Message,
//            Is.EqualTo("Unable to determine if digital identity needs to be updated/created/deleted for customer: "));
//    }

//    [Test]
//    public async Task Generic_MessageNull_ReturnsCouldNotAction()
//    {
//        // Arrange
//        DigitalIdentityClient
//            .Setup(x => x.Post(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
//            .Returns(Task.FromResult(false));
//        RequeueService
//            .Setup(x => x.RequeueItem(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ServiceBusReceivedMessage>()))
//            .Returns(Task.FromResult(false));


//        // Act
//        var response = await SendMessage(null);

//        // Assert
//        Assert.That(DigitalIdentityServiceActions.CouldNotAction, Is.EqualTo(response));
//    }

//    [Test]
//    public async Task Put_UpdateUser_SucceedsAndReturnsSuccessfullyActioned()
//    {
//        // Arrange
//        var identity = new DigitalIdentity { UpdateDigitalIdentity = true };
//        DigitalIdentityClient
//            .Setup(x => x.Put(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
//            .Returns(Task.FromResult(true));
//        var message =
//            ServiceBusModelFactory.ServiceBusReceivedMessage(
//                new BinaryData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity))));
//        // Act
//        var response = await SendMessage(message);

//        // Assert
//        Assert.That(DigitalIdentityServiceActions.SuccessfullyActioned, Is.EqualTo(response));
//    }

//    [Test]
//    public async Task Put_UpdateUser_FailsAndDeadLettersMessage()
//    {
//        // Arrange
//        var identity = new DigitalIdentity { UpdateDigitalIdentity = true };
//        DigitalIdentityClient
//            .Setup(x => x.Put(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
//            .Returns(Task.FromResult(false));
//        var message =
//            ServiceBusModelFactory.ServiceBusReceivedMessage(
//                new BinaryData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity))));
//        // Act
//        var response = await SendMessage(message);

//        // Assert
//        Assert.That(DigitalIdentityServiceActions.DeadLettered, Is.EqualTo(response));
//    }

//    [Test]
//    public async Task Put_UpdateUser_FailsAndReturnsRequeued()
//    {
//        // Arrange
//        var identity = new DigitalIdentity { UpdateDigitalIdentity = true };
//        DigitalIdentityClient
//            .Setup(x => x.Post(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
//            .Returns(Task.FromResult(false));
//        RequeueService
//            .Setup(x => x.RequeueItem(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ServiceBusReceivedMessage>()))
//            .Returns(Task.FromResult(true));
//        var message =
//            ServiceBusModelFactory.ServiceBusReceivedMessage(
//                new BinaryData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity))));

//        // Act
//        var response = await SendMessage(message);

//        // Assert
//        Assert.That(DigitalIdentityServiceActions.Requeued, Is.EqualTo(response));
//    }

//    [Test]
//    public async Task Post_ChangeEmail_SucceedsAndReturnsSuccessfullyActioned()
//    {
//        // Arrange
//        var identity = new DigitalIdentity { ChangeEmailAddress = true };
//        DigitalIdentityClient
//            .Setup(x => x.Post(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
//            .Returns(Task.FromResult(true));
//        var message =
//            ServiceBusModelFactory.ServiceBusReceivedMessage(
//                new BinaryData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity))));
//        // Act
//        var response = await SendMessage(message);

//        // Assert
//        Assert.That(DigitalIdentityServiceActions.SuccessfullyActioned, Is.EqualTo(response));
//    }

//    [Test]
//    public async Task Post_ChangeEmail_FailsAndDeadLettersMessage()
//    {
//        // Arrange
//        var identity = new DigitalIdentity { ChangeEmailAddress = true };
//        DigitalIdentityClient
//            .Setup(x => x.Post(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
//            .Returns(Task.FromResult(false));
//        var message =
//            ServiceBusModelFactory.ServiceBusReceivedMessage(
//                new BinaryData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity))));
//        // Act
//        var response = await SendMessage(message);

//        // Assert
//        Assert.That(DigitalIdentityServiceActions.DeadLettered, Is.EqualTo(response));
//    }

//    [Test]
//    public async Task Post_ChangeEmail_FailsAndReturnsRequeued()
//    {
//        // Arrange
//        var identity = new DigitalIdentity { ChangeEmailAddress = true };
//        DigitalIdentityClient
//            .Setup(x => x.Post(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
//            .Returns(Task.FromResult(false));
//        RequeueService
//            .Setup(x => x.RequeueItem(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<ServiceBusReceivedMessage>()))
//            .Returns(Task.FromResult(true));
//        var message =
//            ServiceBusModelFactory.ServiceBusReceivedMessage(
//                new BinaryData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(identity))));
//        // Act
//        var response = await SendMessage(message);

//        // Assert
//        Assert.That(DigitalIdentityServiceActions.Requeued, Is.EqualTo(response));
//    }
//}