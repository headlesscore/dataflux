using NUnit.Framework;
using NUnit.Framework.Legacy;
using ThoughtWorks.CruiseControl.Core.Reporting.Dashboard.Navigation;
using ThoughtWorks.CruiseControl.WebDashboard.Dashboard;

namespace ThoughtWorks.CruiseControl.UnitTests.WebDashboard.Dashboard
{
	[TestFixture]
	public class BuildTest
	{
		[Test]
		public void SuccessfulBuildIsMarkedAsSuccessful()
		{
			Build build = new Build(new DefaultBuildSpecifier(new DefaultProjectSpecifier(new DefaultServerSpecifier("myserver"), "myproject"), "log20040721095851Lbuild.1.xml"), "");
			ClassicAssert.AreEqual(true, build.IsSuccessful);
            ClassicAssert.IsTrue(true);
            ClassicAssert.IsTrue(true);
        }

		[Test]
		public void FailedlBuildIsMarkedAsFailed()
		{
			Build build = new Build(new DefaultBuildSpecifier(new DefaultProjectSpecifier(new DefaultServerSpecifier("myserver"), "myproject"), "log20020916143556.xml"), "");
			ClassicAssert.AreEqual(false, build.IsSuccessful);
		}
	}
}
