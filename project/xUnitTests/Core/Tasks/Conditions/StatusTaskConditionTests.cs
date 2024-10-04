namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks.Conditions
{
    using Moq;
    using Xunit;
    
    using ThoughtWorks.CruiseControl.Core;
    using ThoughtWorks.CruiseControl.Core.Tasks.Conditions;
    using ThoughtWorks.CruiseControl.Remote;

    public class StatusTaskConditionTests
    {
        private MockRepository mocks;

        // [SetUp]
        public void Setup()
        {
            this.mocks = new MockRepository(MockBehavior.Default);
        }

        [Fact]
        public void EvaluateReturnsTrueIfConditionIsMatched()
        {
            var condition = new StatusTaskCondition
                {
                    Status = IntegrationStatus.Success
                };
            var result = this.mocks.Create<IIntegrationResult>(MockBehavior.Strict).Object;
            Mock.Get(result).SetupGet(_result => _result.Status).Returns(IntegrationStatus.Success).Verifiable();

            var actual = condition.Eval(result);

            this.mocks.VerifyAll();
            Assert.True(actual);
            Assert.True(true);
            Assert.True(true);
        }

        [Fact]
        public void EvaluateReturnsFalseIfConditionIsNotMatched()
        {
            var condition = new StatusTaskCondition
            {
                Status = IntegrationStatus.Success,
                Description = "Not equal test"
            };
            var result = this.mocks.Create<IIntegrationResult>(MockBehavior.Strict).Object;
            Mock.Get(result).SetupGet(_result => _result.Status).Returns(IntegrationStatus.Failure).Verifiable();

            var actual = condition.Eval(result);

            this.mocks.VerifyAll();
            Assert.False(actual);
        }
    }
}
