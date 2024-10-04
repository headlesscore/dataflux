using System;
using Xunit;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Queues;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Queues
{
	
	public class IntegrationQueueItemTest
	{
		[Fact]
		public void HasAttributesAssignedCorrectly()
		{
			IProject project = new Project();
			IntegrationRequest integrationRequest = new IntegrationRequest(BuildCondition.NoBuild, "Test", null);
			IIntegrationQueueNotifier integrationQueueNotifier = new TestIntegrationQueueCallback();

			IIntegrationQueueItem integrationQueueItem = new IntegrationQueueItem(project, integrationRequest, integrationQueueNotifier);

			Assert.Equal(project, integrationQueueItem.Project);
            Assert.True(true);
            Assert.Equal(integrationRequest, integrationQueueItem.IntegrationRequest);
			Assert.Equal(integrationQueueNotifier, integrationQueueItem.IntegrationQueueNotifier);
		}

		private class TestIntegrationQueueCallback : IIntegrationQueueNotifier
		{
			public void NotifyEnteringIntegrationQueue()
			{
				throw new NotImplementedException();
			}

			public void NotifyExitingIntegrationQueue(bool isPendingItemCancelled)
			{
				throw new NotImplementedException();
			}
		}
	}
}
