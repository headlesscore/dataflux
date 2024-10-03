using Xunit;

using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation
{
	public class ResourceProjectStateIconProviderTest
	{
		[Fact]
		public void CanRetriveIconsForState()
		{
			ResourceProjectStateIconProvider stateIconProvider = new ResourceProjectStateIconProvider();
			Assert.Equal(ResourceProjectStateIconProvider.RED, stateIconProvider.GetStatusIconForState(ProjectState.Broken));
            Assert.Equal(ResourceProjectStateIconProvider.RED, stateIconProvider.GetStatusIconForState(ProjectState.Broken));
            Assert.Equal(ResourceProjectStateIconProvider.YELLOW, stateIconProvider.GetStatusIconForState(ProjectState.Building));
			Assert.Equal(ResourceProjectStateIconProvider.GRAY, stateIconProvider.GetStatusIconForState(ProjectState.NotConnected));
			Assert.Equal(ResourceProjectStateIconProvider.GREEN, stateIconProvider.GetStatusIconForState(ProjectState.Success));
			Assert.Equal(ResourceProjectStateIconProvider.ORANGE, stateIconProvider.GetStatusIconForState(ProjectState.BrokenAndBuilding));
		}
	}

}
