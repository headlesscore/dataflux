using Xunit;

using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation
{
	public class ConfigurableProjectStateIconProviderTest
	{
		[Fact]
		public void WhenTheValuesInTheConfigurationAreNullOrEmptyTheDefaultIconsAreUsed()
		{
			Icons icons = new Icons();
			icons.BrokenIcon = string.Empty;
			icons.BuildingIcon = null;

			ConfigurableProjectStateIconProvider stateIconProvider = new ConfigurableProjectStateIconProvider(icons);
			Assert.Same(ResourceProjectStateIconProvider.RED, stateIconProvider.GetStatusIconForState(ProjectState.Broken));
            //ClassicAssert.AreSame(ResourceProjectStateIconProvider.RED, stateIconProvider.GetStatusIconForState(ProjectState.Broken));
            Assert.Same(ResourceProjectStateIconProvider.YELLOW, stateIconProvider.GetStatusIconForState(ProjectState.Building));
			Assert.Same(ResourceProjectStateIconProvider.GRAY, stateIconProvider.GetStatusIconForState(ProjectState.NotConnected));
			Assert.Same(ResourceProjectStateIconProvider.GREEN, stateIconProvider.GetStatusIconForState(ProjectState.Success));
			Assert.Same(ResourceProjectStateIconProvider.ORANGE, stateIconProvider.GetStatusIconForState(ProjectState.BrokenAndBuilding));
		}
	}
}
