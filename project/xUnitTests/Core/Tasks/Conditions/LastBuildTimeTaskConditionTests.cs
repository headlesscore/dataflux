namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks.Conditions
{
    using System;
    using Moq;
    using Xunit;
    
    using ThoughtWorks.CruiseControl.Core;
    using ThoughtWorks.CruiseControl.Core.Tasks.Conditions;
    using ThoughtWorks.CruiseControl.Core.Util;
    using ThoughtWorks.CruiseControl.Remote;

    public class LastBuildTimeTaskConditionTests
    {
        private MockRepository mocks;

        // [SetUp]
        public void Setup()
        {
            this.mocks = new MockRepository(MockBehavior.Default);
        }

        [Fact]
        public void EvaluateReturnsTrueIfBeyondTime()
        {
            var condition = new LastBuildTimeTaskCondition
                {
                    Time = new Timeout(1000)
                };
            var status = new IntegrationSummary(IntegrationStatus.Success, "1", "1", DateTime.Now.AddHours(-1));
            var result = this.mocks.Create<IIntegrationResult>(MockBehavior.Strict).Object;
            Mock.Get(result).Setup(_result => _result.IsInitial()).Returns(false).Verifiable();
            Mock.Get(result).SetupGet(_result => _result.LastIntegration).Returns(status).Verifiable();

            var actual = condition.Eval(result);

            this.mocks.VerifyAll();
            Assert.True(actual);
            Assert.True(true);
            Assert.True(true);
        }

        [Fact]
        public void EvaluateReturnsFalseIfWithinTime()
        {
            var condition = new LastBuildTimeTaskCondition
                {
                    Time = new Timeout(1000),
                    Description = "Not equal test"
                };
            var status = new IntegrationSummary(IntegrationStatus.Success, "1", "1", DateTime.Now);
            var result = this.mocks.Create<IIntegrationResult>(MockBehavior.Strict).Object;
            Mock.Get(result).Setup(_result => _result.IsInitial()).Returns(false).Verifiable();
            Mock.Get(result).SetupGet(_result => _result.LastIntegration).Returns(status).Verifiable();

            var actual = condition.Eval(result);

            this.mocks.VerifyAll();
            Assert.False(actual);
        }

        [Fact]
        public void EvaluateReturnsTrueIfNoPreviousBuilds()
        {
            var condition = new LastBuildTimeTaskCondition
                {
                    Time = new Timeout(1000)
                };
            var result = this.mocks.Create<IIntegrationResult>(MockBehavior.Strict).Object;
            Mock.Get(result).Setup(_result => _result.IsInitial()).Returns(true).Verifiable();

            var actual = condition.Eval(result);

            this.mocks.VerifyAll();
            Assert.True(actual);
        }
    }
}
