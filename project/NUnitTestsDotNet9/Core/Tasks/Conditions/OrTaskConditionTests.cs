﻿namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks.Conditions
{
    using System;
    using Moq;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using ThoughtWorks.CruiseControl.Core;
    using ThoughtWorks.CruiseControl.Core.Config;
    using ThoughtWorks.CruiseControl.Core.Tasks;
    using ThoughtWorks.CruiseControl.Core.Tasks.Conditions;

    public class OrTaskConditionTests
    {
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            this.mocks = new MockRepository(MockBehavior.Default);
        }

        [Test]
        public void ValidateRaisesAnErrorIfNullChildConditionsDefined()
        {
            var processor = this.mocks.Create<IConfigurationErrorProcesser>(MockBehavior.Strict).Object;
            Mock.Get(processor).Setup(_processor => _processor.ProcessError(
                "Validation failed for orCondition - at least one child condition must be supplied")).Verifiable();

            var condition = new OrTaskCondition();
            condition.Validate(null, null, processor);

            this.mocks.VerifyAll();
        }

        [Test]
        public void ValidateRaisesAnErrorIfNoChildConditionsDefined()
        {
            var processor = this.mocks.Create<IConfigurationErrorProcesser>(MockBehavior.Strict).Object;
            Mock.Get(processor).Setup(_processor => _processor.ProcessError(
                "Validation failed for orCondition - at least one child condition must be supplied")).Verifiable();

            var condition = new OrTaskCondition
            {
                Conditions = new ITaskCondition[0]
            };
            condition.Validate(null, null, processor);

            this.mocks.VerifyAll();
        }

        [Test]
        public void ValidateCallsChildrenValidation()
        {
            var processor = this.mocks.Create<IConfigurationErrorProcesser>(MockBehavior.Strict).Object;
            var validateCalled = false;
            var childCondition = new MockCondition
            {
                ValidateAction = (c, t, ep) => validateCalled = true
            };

            var condition = new OrTaskCondition
            {
                Conditions = new[] { childCondition }
            };
            condition.Validate(null, ConfigurationTrace.Start(this), processor);

            this.mocks.VerifyAll();
            ClassicAssert.IsTrue(validateCalled);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void EvaluateReturnsFalseIfAllChildrenAreFalse()
        {
            Func<IIntegrationResult, bool> evalFunc = ir => false;
            var condition = new OrTaskCondition
            {
                Conditions = new[] 
                        {
                            new MockCondition { EvalFunction = evalFunc },
                            new MockCondition { EvalFunction = evalFunc }
                        }
            };
            var result = this.mocks.Create<IIntegrationResult>(MockBehavior.Strict).Object;

            var actual = condition.Eval(result);

            this.mocks.VerifyAll();
            ClassicAssert.IsFalse(actual);
        }

        [Test]
        public void EvaluateReturnsTrueIfOneChildIsTrue()
        {
            var count = 0;
            Func<IIntegrationResult, bool> evalFunc = ir => (count++) % 2 == 0;
            var condition = new OrTaskCondition
            {
                Conditions = new[] 
                        {
                            new MockCondition { EvalFunction = evalFunc },
                            new MockCondition { EvalFunction = evalFunc }
                        }
            };
            var result = this.mocks.Create<IIntegrationResult>(MockBehavior.Strict).Object;

            var actual = condition.Eval(result);

            this.mocks.VerifyAll();
            ClassicAssert.IsTrue(actual);
        }
    }
}
