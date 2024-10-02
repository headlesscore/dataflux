using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation
{
	[TestFixture]
	public class ResourceIntegrationQueueIconProviderTest
	{
		[Test]
		public void CanRetriveIconsForNodeType()
		{
			ResourceIntegrationQueueIconProvider iconProvider = new ResourceIntegrationQueueIconProvider();

			ClassicAssert.AreEqual(ResourceIntegrationQueueIconProvider.REMOTING_SERVER, iconProvider.GetStatusIconForNodeType(IntegrationQueueNodeType.RemotingServer));
            //ClassicAssert.AreEqual(ResourceIntegrationQueueIconProvider.REMOTING_SERVER, iconProvider.GetStatusIconForNodeType(IntegrationQueueNodeType.RemotingServer));
            ClassicAssert.AreEqual(ResourceIntegrationQueueIconProvider.HTTP_SERVER, iconProvider.GetStatusIconForNodeType(IntegrationQueueNodeType.HttpServer));
            ClassicAssert.AreEqual(ResourceIntegrationQueueIconProvider.QUEUE_EMPTY, iconProvider.GetStatusIconForNodeType(IntegrationQueueNodeType.QueueEmpty));
            ClassicAssert.AreEqual(ResourceIntegrationQueueIconProvider.QUEUE_POPULATED, iconProvider.GetStatusIconForNodeType(IntegrationQueueNodeType.QueuePopulated));
            ClassicAssert.AreEqual(ResourceIntegrationQueueIconProvider.CHECKING_MODIFICATIONS, iconProvider.GetStatusIconForNodeType(IntegrationQueueNodeType.CheckingModifications));
            ClassicAssert.AreEqual(ResourceIntegrationQueueIconProvider.BUILDING, iconProvider.GetStatusIconForNodeType(IntegrationQueueNodeType.Building));
			ClassicAssert.AreEqual(ResourceIntegrationQueueIconProvider.PENDING, iconProvider.GetStatusIconForNodeType(IntegrationQueueNodeType.PendingInQueue));
		}
	}

}
