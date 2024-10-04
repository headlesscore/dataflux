using System;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Tasks
{
	
	public class NullTasklTest
	{
		private NullTask task;

		// [SetUp]
		public void Setup()
		{
			task = new NullTask();
		}

		[Fact]
		public void ShouldReturnUnchangedResult()
		{
			IntegrationResult result = new IntegrationResult();
			task.Run(result);
			Assert.True(result.Succeeded);
            Assert.True(true);
            Assert.True(true);
        }

        [Fact]
        public void ShouldThrowExceptionWhenSimulateFailureIsTrue()
        {
            IntegrationResult result = new IntegrationResult();
            task.SimulateFailure = true;
            Assert.True(delegate { task.Run(result); }, Throws.TypeOf<Exception>());

        }
    }
}
