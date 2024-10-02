namespace ThoughtWorks.CruiseControl.UnitTests.Core.Triggers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Moq;
    using NUnit.Framework;
    using ThoughtWorks.CruiseControl.Remote;
    using ThoughtWorks.CruiseControl.Core.Triggers;
    using NUnit.Framework.Legacy;

    public class ParameterTriggerTests
    {
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            this.mocks = new MockRepository(MockBehavior.Default);
        }

        [Test]
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

        [Test]
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
            ClassicAssert.AreEqual(now, actual);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
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
            ClassicAssert.IsNull(actual);
        }

        [Test]
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
            ClassicAssert.AreSame(request, actual);
            ClassicAssert.AreEqual(1, request.BuildValues.Count);
            ClassicAssert.AreEqual(parameters[0].Value,
                request.BuildValues[parameters[0].Name]);
        }

        [Test]
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
            ClassicAssert.AreSame(request, actual);
            ClassicAssert.AreEqual(0, request.BuildValues.Count);
        }
    }
}
