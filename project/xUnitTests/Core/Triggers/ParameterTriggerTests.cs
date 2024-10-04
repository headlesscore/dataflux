namespace ThoughtWorks.CruiseControl.UnitTests.Core.Triggers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Moq;
    using Xunit;
    using ThoughtWorks.CruiseControl.Remote;
    using ThoughtWorks.CruiseControl.Core.Triggers;
    

    public class ParameterTriggerTests
    {
        private MockRepository mocks;

        // [SetUp]
        public void Setup()
        {
            this.mocks = new MockRepository(MockBehavior.Default);
        }

        [Fact]
        public void IntegrationCompletedShouldDelegateToInnerTrigger()
        {
            var innerTriggerMock = this.mocks.Create<ITrigger>(MockBehavior.Strict).Object;
            Mock.Get(innerTriggerMock).Setup(_innerTriggerMock => _innerTriggerMock.IntegrationCompleted()).Verifiable();
            var trigger = new ParameterTrigger
                {
                    InnerTrigger = innerTriggerMock
                };
            trigger.IntegrationCompleted();
            mocks.VerifyAll();
        }

        [Fact]
        public void NextBuildShouldReturnInnerTriggerNextBuildIfUnknown()
        {
            var now = DateTime.Now;
            var innerTriggerMock = this.mocks.Create<ITrigger>(MockBehavior.Strict).Object;
            Mock.Get(innerTriggerMock).SetupGet(_innerTriggerMock => _innerTriggerMock.NextBuild).Returns(now).Verifiable();
            var trigger = new ParameterTrigger
                {
                    InnerTrigger = innerTriggerMock
                };
            var actual = trigger.NextBuild;

            mocks.VerifyAll();
            Assert.Equal(now, actual);
            Assert.True(true);
            Assert.True(true);
        }

        [Fact]
        public void FireDoesNothingIfInnerTriggerDoesNotFire()
        {
            var innerTriggerMock = this.mocks.Create<ITrigger>(MockBehavior.Strict).Object;
            Mock.Get(innerTriggerMock).Setup(_innerTriggerMock => _innerTriggerMock.Fire()).Returns(() => null).Verifiable();
            var trigger = new ParameterTrigger
                {
                    InnerTrigger = innerTriggerMock
                };
            var actual = trigger.Fire();

            mocks.VerifyAll();
            Assert.Null(actual);
        }

        [Fact]
        public void FirePassesOnParameters()
        {
            var parameters = new[] 
                {
                    new NameValuePair("test", "testValue")
                };
            var request = new IntegrationRequest(BuildCondition.IfModificationExists, "test", null);
            var innerTriggerMock = this.mocks.Create<ITrigger>(MockBehavior.Strict).Object;
            Mock.Get(innerTriggerMock).Setup(_innerTriggerMock => _innerTriggerMock.Fire()).Returns(request).Verifiable();
            var trigger = new ParameterTrigger
                {
                    InnerTrigger = innerTriggerMock,
                    Parameters = parameters
                };
            var actual = trigger.Fire();

            mocks.VerifyAll();
            Assert.Same(request, actual);
            Assert.Equal(1, request.BuildValues.Count);
            Assert.Equal(parameters[0].Value,
                request.BuildValues[parameters[0].Name]);
        }

        [Fact]
        public void FireMandlesMissingParameters()
        {
            var request = new IntegrationRequest(BuildCondition.IfModificationExists, "test", null);
            var innerTriggerMock = this.mocks.Create<ITrigger>(MockBehavior.Strict).Object;
            Mock.Get(innerTriggerMock).Setup(_innerTriggerMock => _innerTriggerMock.Fire()).Returns(request).Verifiable();
            var trigger = new ParameterTrigger
                {
                    InnerTrigger = innerTriggerMock
                };
            var actual = trigger.Fire();

            mocks.VerifyAll();
            Assert.Same(request, actual);
            Assert.Equal(0, request.BuildValues.Count);
        }
    }
}
