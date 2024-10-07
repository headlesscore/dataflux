namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks.Conditions
{
    using System;
    using Moq;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Core;
    using ThoughtWorks.CruiseControl.Core.Tasks.Conditions;

    public class CompareValuesTaskConditionTests
    {
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            this.mocks = new MockRepository(MockBehavior.Default);
        }

        [Test]
        public void EvaluateReturnsTrueIfValuesMatch()
        {
            var condition = new CompareValuesTaskCondition
                {
                    Value1 = "test",
                    Value2 = "test"
                };
            var result = this.mocks.Create<IIntegrationResult>(MockBehavior.Strict).Object;

            var actual = condition.Eval(result);

            this.mocks.VerifyAll();
            ClassicAssert.IsTrue(actual);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void EvaluateReturnsFalseIfValuesDontMatch()
        {
            var condition = new CompareValuesTaskCondition
                {
                    Value1 = "test1",
                    Value2 = "test2"
                };
            var result = this.mocks.Create<IIntegrationResult>(MockBehavior.Strict).Object;

            var actual = condition.Eval(result);

            this.mocks.VerifyAll();
            ClassicAssert.IsFalse(actual);
        }

        [Test]
        public void EvaluateReturnsTrueIfValuesDontMatchAndTypeIsNotEqual()
        {
            var condition = new CompareValuesTaskCondition
                {
                    Value1 = "test1",
                    Value2 = "test2",
                    EvaluationType = CompareValuesTaskCondition.Evaluation.NotEqual
                };
            var result = this.mocks.Create<IIntegrationResult>(MockBehavior.Strict).Object;

            var actual = condition.Eval(result);

            this.mocks.VerifyAll();
            ClassicAssert.IsTrue(actual);
        }

        [Test]
        public void EvaluateFailsIfTypeIsUnknown()
        {
            var condition = new CompareValuesTaskCondition
            {
                Value1 = "test1",
                Value2 = "test2",
                EvaluationType = (CompareValuesTaskCondition.Evaluation)99
            };
            var result = this.mocks.Create<IIntegrationResult>(MockBehavior.Strict).Object;

            ClassicAssert.Throws<InvalidOperationException>(() => condition.Eval(result));

            this.mocks.VerifyAll();
        }
    }
}
