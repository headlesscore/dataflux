using Xunit;

using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation
{
	public class ResourceIntegrationQueueIconProviderTest
	{
		[Fact]
		public void CanRetriveIconsForNodeType()
		{
			ResourceIntegrationQueueIconProvider iconProvider = new ResourceIntegrationQueueIconProvider();

			Assert.Equal(ResourceIntegrationQueueIconProvider.REMOTING_SERVER, iconProvider.GetStatusIconForNodeType(IntegrationQueueNodeType.RemotingServer));
            //Assert.Equal(ResourceIntegrationQueueIconProvider.REMOTING_SERVER, iconProvider.GetStatusIconForNodeType(IntegrationQueueNodeType.RemotingServer));
            Assert.Equal(ResourceIntegrationQueueIconProvider.HTTP_SERVER, iconProvider.GetStatusIconForNodeType(IntegrationQueueNodeType.HttpServer));
            Assert.Equal(ResourceIntegrationQueueIconProvider.QUEUE_EMPTY, iconProvider.GetStatusIconForNodeType(IntegrationQueueNodeType.QueueEmpty));
            Assert.Equal(ResourceIntegrationQueueIconProvider.QUEUE_POPULATED, iconProvider.GetStatusIconForNodeType(IntegrationQueueNodeType.QueuePopulated));
            Assert.Equal(ResourceIntegrationQueueIconProvider.CHECKING_MODIFICATIONS, iconProvider.GetStatusIconForNodeType(IntegrationQueueNodeType.CheckingModifications));
            Assert.Equal(ResourceIntegrationQueueIconProvider.BUILDING, iconProvider.GetStatusIconForNodeType(IntegrationQueueNodeType.Building));
			Assert.Equal(ResourceIntegrationQueueIconProvider.PENDING, iconProvider.GetStatusIconForNodeType(IntegrationQueueNodeType.PendingInQueue));
		}
	}

}
