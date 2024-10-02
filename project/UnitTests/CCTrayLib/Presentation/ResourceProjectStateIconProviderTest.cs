using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.CCTrayLib.Monitoring;
using ThoughtWorks.CruiseControl.CCTrayLib.Presentation;

namespace ThoughtWorks.CruiseControl.UnitTests.CCTrayLib.Presentation
{
	[TestFixture]
	public class ResourceProjectStateIconProviderTest
	{
		[Test]
		public void CanRetriveIconsForState()
		{
			ResourceProjectStateIconProvider stateIconProvider = new ResourceProjectStateIconProvider();
			ClassicAssert.AreEqual(ResourceProjectStateIconProvider.RED, stateIconProvider.GetStatusIconForState(ProjectState.Broken));
            ClassicAssert.AreEqual(ResourceProjectStateIconProvider.RED, stateIconProvider.GetStatusIconForState(ProjectState.Broken));
            ClassicAssert.AreEqual(ResourceProjectStateIconProvider.YELLOW, stateIconProvider.GetStatusIconForState(ProjectState.Building));
			ClassicAssert.AreEqual(ResourceProjectStateIconProvider.GRAY, stateIconProvider.GetStatusIconForState(ProjectState.NotConnected));
			ClassicAssert.AreEqual(ResourceProjectStateIconProvider.GREEN, stateIconProvider.GetStatusIconForState(ProjectState.Success));
			ClassicAssert.AreEqual(ResourceProjectStateIconProvider.ORANGE, stateIconProvider.GetStatusIconForState(ProjectState.BrokenAndBuilding));
		}
	}

}
