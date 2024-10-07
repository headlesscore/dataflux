namespace ThoughtWorks.CruiseControl.UnitTests.Core.Extensions
{
    using System;
    using System.Xml;
    using Moq;
    using NUnit.Framework;
    using CruiseControl.Core.Extensions;
    using CruiseControl.Remote;
    using CruiseControl.Remote.Events;
    using NUnit.Framework.Legacy;

    [TestFixture]
    public class IntegrationRequestThrottleExtensionTests
    {
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository(MockBehavior.Default);
        }

        [Test]
        public void InitialiseLoadsNumberOfRequestsAllowed()
        {
            var extension = new IntegrationRequestThrottleExtension();
            var serverMock = this.mocks.Create<ICruiseServer>(MockBehavior.Strict).Object;
            var config = new ExtensionConfiguration();
            config.Items = new[]
                {
                    GenerateElement("limit", "10")
                };
            Mock.Get(serverMock).SetupAdd(_serverMock => _serverMock.IntegrationCompleted += It.IsAny<EventHandler<IntegrationCompletedEventArgs>>()).Verifiable();
            Mock.Get(serverMock).SetupAdd(_serverMock => _serverMock.IntegrationStarted += It.IsAny<EventHandler<IntegrationStartedEventArgs>>()).Verifiable();

            extension.Initialise(serverMock, config);

            mocks.VerifyAll();
            ClassicAssert.AreEqual(10, extension.NumberOfRequestsAllowed);
            ClassicAssert.AreEqual(10, extension.NumberOfRequestsAllowed);
        }

        [Test]
        public void IntegrationStartIsAllowedWhenWithinLimit()
        {
            var extension = new IntegrationRequestThrottleExtension();
            var serverMock = this.mocks.Create<ICruiseServer>(MockBehavior.Strict).Object;
            var config = new ExtensionConfiguration();
            Mock.Get(serverMock).SetupAdd(_serverMock => _serverMock.IntegrationCompleted += It.IsAny<EventHandler<IntegrationCompletedEventArgs>>()).Verifiable();
            Mock.Get(serverMock).SetupAdd(_serverMock => _serverMock.IntegrationStarted += It.IsAny<EventHandler<IntegrationStartedEventArgs>>()).Verifiable();
            var eventArgs = new IntegrationStartedEventArgs(null, "TestProject");

            extension.Initialise(serverMock, config);
            Mock.Get(serverMock).Raise(_serverMock => _serverMock.IntegrationStarted += null, eventArgs);

            mocks.VerifyAll();
            ClassicAssert.AreEqual(IntegrationStartedEventArgs.EventResult.Continue, eventArgs.Result);
        }

        [Test]
        public void IntegrationStartIsDelayedBeyondLimit()
        {
            var extension = new IntegrationRequestThrottleExtension();
            var serverMock = this.mocks.Create<ICruiseServer>(MockBehavior.Strict).Object;
            var config = new ExtensionConfiguration();
            Mock.Get(serverMock).SetupAdd(_serverMock => _serverMock.IntegrationCompleted += It.IsAny<EventHandler<IntegrationCompletedEventArgs>>()).Verifiable();
            Mock.Get(serverMock).SetupAdd(_serverMock => _serverMock.IntegrationStarted += It.IsAny<EventHandler<IntegrationStartedEventArgs>>()).Verifiable();
            var eventArgs = new IntegrationStartedEventArgs(null, "TestProject");

            extension.Initialise(serverMock, config);
            extension.NumberOfRequestsAllowed = 1;
            Mock.Get(serverMock).Raise(_serverMock => _serverMock.IntegrationStarted += null, new IntegrationStartedEventArgs(null, "First"));
            Mock.Get(serverMock).Raise(_serverMock => _serverMock.IntegrationStarted += null, eventArgs);

            mocks.VerifyAll();
            ClassicAssert.AreEqual(IntegrationStartedEventArgs.EventResult.Delay, eventArgs.Result);
        }

        [Test]
        public void IntegrationStartAllowedAtTopOfQueue()
        {
            var extension = new IntegrationRequestThrottleExtension();
            var serverMock = this.mocks.Create<ICruiseServer>(MockBehavior.Strict).Object;
            var config = new ExtensionConfiguration();
            Mock.Get(serverMock).SetupAdd(_serverMock => _serverMock.IntegrationCompleted += It.IsAny<EventHandler<IntegrationCompletedEventArgs>>()).Verifiable();
            Mock.Get(serverMock).SetupAdd(_serverMock => _serverMock.IntegrationStarted += It.IsAny<EventHandler<IntegrationStartedEventArgs>>()).Verifiable();
            var eventArgs = new IntegrationStartedEventArgs(null, "TestProject");

            extension.Initialise(serverMock, config);
            extension.NumberOfRequestsAllowed = 1;
            Mock.Get(serverMock).Raise(_serverMock => _serverMock.IntegrationStarted += null, new IntegrationStartedEventArgs(null, "First"));
            Mock.Get(serverMock).Raise(_serverMock => _serverMock.IntegrationStarted += null, eventArgs);
            Mock.Get(serverMock).Raise(_serverMock => _serverMock.IntegrationStarted += null, new IntegrationStartedEventArgs(null, "Third"));
            Mock.Get(serverMock).Raise(_serverMock => _serverMock.IntegrationCompleted += null, new IntegrationCompletedEventArgs(null, "First", IntegrationStatus.Success));
            Mock.Get(serverMock).Raise(_serverMock => _serverMock.IntegrationStarted += null, eventArgs);

            mocks.VerifyAll();
            ClassicAssert.AreEqual(IntegrationStartedEventArgs.EventResult.Continue, eventArgs.Result);
        }

        [Test]
        public void IntegrationCompleteClearsSlot()
        {
            var extension = new IntegrationRequestThrottleExtension();
            var serverMock = this.mocks.Create<ICruiseServer>(MockBehavior.Strict).Object;
            var config = new ExtensionConfiguration();
            Mock.Get(serverMock).SetupAdd(_serverMock => _serverMock.IntegrationCompleted += It.IsAny<EventHandler<IntegrationCompletedEventArgs>>()).Verifiable();
            Mock.Get(serverMock).SetupAdd(_serverMock => _serverMock.IntegrationStarted += It.IsAny<EventHandler<IntegrationStartedEventArgs>>()).Verifiable();
            var eventArgs = new IntegrationStartedEventArgs(null, "TestProject");

            extension.Initialise(serverMock, config);
            extension.NumberOfRequestsAllowed = 1;
            Mock.Get(serverMock).Raise(_serverMock => _serverMock.IntegrationStarted += null, new IntegrationStartedEventArgs(null, "First"));
            Mock.Get(serverMock).Raise(_serverMock => _serverMock.IntegrationStarted += null, eventArgs);
            Mock.Get(serverMock).Raise(_serverMock => _serverMock.IntegrationCompleted += null, new IntegrationCompletedEventArgs(null, "First", IntegrationStatus.Success));
            Mock.Get(serverMock).Raise(_serverMock => _serverMock.IntegrationStarted += null, eventArgs);

            mocks.VerifyAll();
            ClassicAssert.AreEqual(IntegrationStartedEventArgs.EventResult.Continue, eventArgs.Result);
        }

        [Test]
        public void StartAndStopDoesNothing()
        {
            var extension = new IntegrationRequestThrottleExtension();
            var serverMock = this.mocks.Create<ICruiseServer>(MockBehavior.Strict).Object;
            var config = new ExtensionConfiguration();
            Mock.Get(serverMock).SetupAdd(_serverMock => _serverMock.IntegrationCompleted += It.IsAny<EventHandler<IntegrationCompletedEventArgs>>()).Verifiable();
            Mock.Get(serverMock).SetupAdd(_serverMock => _serverMock.IntegrationStarted += It.IsAny<EventHandler<IntegrationStartedEventArgs>>()).Verifiable();

            extension.Initialise(serverMock, config);
            extension.Start();
            extension.Stop();

            mocks.VerifyAll();
        }

        [Test]
        public void StartAndAbortDoesNothing()
        {
            var extension = new IntegrationRequestThrottleExtension();
            var serverMock = this.mocks.Create<ICruiseServer>(MockBehavior.Strict).Object;
            var config = new ExtensionConfiguration();
            Mock.Get(serverMock).SetupAdd(_serverMock => _serverMock.IntegrationCompleted += It.IsAny<EventHandler<IntegrationCompletedEventArgs>>()).Verifiable();
            Mock.Get(serverMock).SetupAdd(_serverMock => _serverMock.IntegrationStarted += It.IsAny<EventHandler<IntegrationStartedEventArgs>>()).Verifiable();

            extension.Initialise(serverMock, config);
            extension.Start();
            extension.Abort();

            mocks.VerifyAll();
        }

        private XmlElement GenerateElement(string name, string value)
        {
            var xmlDoc = new XmlDocument();
            var element = xmlDoc.CreateElement(name);
            element.InnerXml = value;

            return element;
        }
    }
}
