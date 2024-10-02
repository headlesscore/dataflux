using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.CCTrayLib.Configuration;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation
{
	[TestFixture]
	public class ConfigurableProjectStateIconProviderTest
	{
		[Test]
		public void WhenTheValuesInTheConfigurationAreNullOrEmptyTheDefaultIconsAreUsed()
		{
			Icons icons = new Icons();
			icons.BrokenIcon = string.Empty;
			icons.BuildingIcon = null;

			ConfigurableProjectStateIconProvider stateIconProvider = new ConfigurableProjectStateIconProvider(icons);
			ClassicAssert.AreSame(ResourceProjectStateIconProvider.RED, stateIconProvider.GetStatusIconForState(ProjectState.Broken));
            //ClassicAssert.AreSame(ResourceProjectStateIconProvider.RED, stateIconProvider.GetStatusIconForState(ProjectState.Broken));
            ClassicAssert.AreSame(ResourceProjectStateIconProvider.YELLOW, stateIconProvider.GetStatusIconForState(ProjectState.Building));
			ClassicAssert.AreSame(ResourceProjectStateIconProvider.GRAY, stateIconProvider.GetStatusIconForState(ProjectState.NotConnected));
			ClassicAssert.AreSame(ResourceProjectStateIconProvider.GREEN, stateIconProvider.GetStatusIconForState(ProjectState.Success));
			ClassicAssert.AreSame(ResourceProjectStateIconProvider.ORANGE, stateIconProvider.GetStatusIconForState(ProjectState.BrokenAndBuilding));
		}
	}
}
