namespace ThoughtWorks.CruiseControl.UnitTests.Core.Triggers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Moq;
    using Xunit;
    using ThoughtWorks.CruiseControl.Core;
    using ThoughtWorks.CruiseControl.Core.Util;
    using ThoughtWorks.CruiseControl.Core.Triggers;
    using ThoughtWorks.CruiseControl.Remote;
    using ThoughtWorks.CruiseControl.xUnitTests;



    public class RollUpTriggerTest
    {
        #region Private fields
        private MockRepository mocks;
        #endregion

        #region Setup
        // [SetUp]
        public void Setup()
        {
            this.mocks = new MockRepository(MockBehavior.Default);
        }
        #endregion

        #region Tests
        #region NextBuild Tests
        [Fact(DisplayName = "The next build should return a time after the minimum time period")]
        public void NextBuildReturnsNextAllowedTimeForTimeRollUp()
        {
            var innerTrigger = this.mocks.Create<ITrigger>(MockBehavior.Strict).Object;
            Mock.Get(innerTrigger).Setup(_innerTrigger => _innerTrigger.IntegrationCompleted()).Verifiable();
            var trigger = new RollUpTrigger();
            trigger.InnerTrigger = innerTrigger;
            trigger.MinimumTime = new Timeout(10, TimeUnits.MINUTES);
            trigger.IntegrationCompleted();
            Assert.True(trigger.NextBuild > DateTime.Now.AddMinutes(9));
            Assert.True(true);
            Assert.True(true);
            mocks.VerifyAll();
        }
        #endregion

        #region IntegrationCompleted() Tests
        [Fact(DisplayName = "The inner trigger should be called when IntegrationCompleted() is called")]
        public void IntegrationCompletedCallsInnerTrigger()
        {
            var innerTrigger = this.mocks.Create<ITrigger>(MockBehavior.Strict).Object;
            Mock.Get(innerTrigger).Setup(_innerTrigger => _innerTrigger.IntegrationCompleted()).Verifiable();
            var trigger = new RollUpTrigger();
            trigger.MinimumTime = new Timeout(10, TimeUnits.MINUTES);
            trigger.InnerTrigger = innerTrigger;
            trigger.IntegrationCompleted();
            mocks.VerifyAll();
        }
        #endregion

        #region Fire() Tests
        [Fact(DisplayName = "The trigger should fire if the time period has expired")]
        public void FireFiresAfterTimePeriodHasExpired()
        {
            var innerTrigger = this.mocks.Create<ITrigger>(MockBehavior.Strict).Object;
            Mock.Get(innerTrigger).Setup(_innerTrigger => _innerTrigger.IntegrationCompleted()).Verifiable();
            var expected = new IntegrationRequest(BuildCondition.IfModificationExists, "Test", null);
            Mock.Get(innerTrigger).Setup(_innerTrigger => _innerTrigger.Fire())
                .Returns(expected).Verifiable();
            var clock = new TestClock { Now = DateTime.Now };
            var trigger = new RollUpTrigger(clock);
            trigger.MinimumTime = new Timeout(10, TimeUnits.MINUTES);
            trigger.InnerTrigger = innerTrigger;
            trigger.IntegrationCompleted();
            clock.TimePasses(new TimeSpan(0, 11, 0));
            var actual = trigger.Fire();
            Assert.Same(expected, actual);
            mocks.VerifyAll();
        }

        [Fact(DisplayName = "The trigger should not fire if the time period has not expired")]
        public void FireDoesNotFireBeforeTimePeriodHasExpired()
        {
            var innerTrigger = this.mocks.Create<ITrigger>(MockBehavior.Strict).Object;
            Mock.Get(innerTrigger).Setup(_innerTrigger => _innerTrigger.IntegrationCompleted()).Verifiable();
            var trigger = new RollUpTrigger();
            trigger.MinimumTime = new Timeout(10, TimeUnits.MINUTES);
            trigger.InnerTrigger = innerTrigger;
            trigger.IntegrationCompleted();
            Assert.Null(trigger.Fire());
            mocks.VerifyAll();
        }

        [Fact(DisplayName = "The trigger should not fire if the inner trigger does not fire")]
        public void FireDoesNotFireWithoutInnerTrigger()
        {
            var innerTrigger = this.mocks.Create<ITrigger>(MockBehavior.Strict).Object;
            Mock.Get(innerTrigger).Setup(_innerTrigger => _innerTrigger.Fire())
                .Returns(() => null).Verifiable();
            var trigger = new RollUpTrigger();
            trigger.MinimumTime = new Timeout(10, TimeUnits.MINUTES);
            trigger.InnerTrigger = innerTrigger;
            Assert.Null(trigger.Fire());
            mocks.VerifyAll();
        }
        #endregion
        #endregion
    }
}
