namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks.Conditions
{
    using Moq;
    using Xunit;
    
    using ThoughtWorks.CruiseControl.Core;
    using ThoughtWorks.CruiseControl.Core.Tasks.Conditions;
    using ThoughtWorks.CruiseControl.Remote;

    public class BuildConditionTaskConditionTests
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
            var condition = new BuildConditionTaskCondition
            {
                BuildCondition = BuildCondition.ForceBuild
            };
            var result = this.mocks.Create<IIntegrationResult>(MockBehavior.Strict).Object;
            Mock.Get(result).SetupGet(_result => _result.BuildCondition).Returns(BuildCondition.ForceBuild).Verifiable();

            var actual = condition.Eval(result);

            this.mocks.VerifyAll();
            Assert.True(actual);
            Assert.True(true);
            Assert.True(true);
        }

        [Fact]
        public void EvaluateReturnsFalseIfConditionIsNotMatched()
        {
            var condition = new BuildConditionTaskCondition
                {
                    BuildCondition = BuildCondition.ForceBuild,
                    Description = "Not equal test"
                };
            var result = this.mocks.Create<IIntegrationResult>(MockBehavior.Strict).Object;
            Mock.Get(result).SetupGet(_result => _result.BuildCondition).Returns(BuildCondition.IfModificationExists).Verifiable();

            var actual = condition.Eval(result);

            this.mocks.VerifyAll();
            Assert.False(actual);
        }
    }
}
