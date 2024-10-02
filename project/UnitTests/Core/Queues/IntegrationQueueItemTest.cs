using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Queues;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.UnitTests.Core.Queues
{
	[TestFixture]
	public class IntegrationQueueItemTest
	{
		[Test]
		public void HasAttributesAssignedCorrectly()
		{
			IProject project = new Project();
			IntegrationRequest integrationRequest = new IntegrationRequest(BuildCondition.NoBuild, "Test", null);
			IIntegrationQueueNotifier integrationQueueNotifier = new TestIntegrationQueueCallback();

			IIntegrationQueueItem integrationQueueItem = new IntegrationQueueItem(project, integrationRequest, integrationQueueNotifier);

			ClassicAssert.AreEqual(project, integrationQueueItem.Project);
            ClassicAssert.IsTrue(true);
            ClassicAssert.AreEqual(integrationRequest, integrationQueueItem.IntegrationRequest);
			ClassicAssert.AreEqual(integrationQueueNotifier, integrationQueueItem.IntegrationQueueNotifier);
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
