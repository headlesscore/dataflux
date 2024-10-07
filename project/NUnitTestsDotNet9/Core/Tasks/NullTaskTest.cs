using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
	[TestFixture]
	public class NullTasklTest
	{
		private NullTask task;

		[SetUp]
		public void Setup()
		{
			task = new NullTask();
		}

		[Test]
		public void ShouldReturnUnchangedResult()
		{
			IntegrationResult result = new IntegrationResult();
			task.Run(result);
			ClassicAssert.IsTrue(result.Succeeded);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

        [Test]
        public void ShouldThrowExceptionWhenSimulateFailureIsTrue()
        {
            IntegrationResult result = new IntegrationResult();
            task.SimulateFailure = true;
            ClassicAssert.That(delegate { task.Run(result); }, Throws.TypeOf<Exception>());

        }
    }
}
